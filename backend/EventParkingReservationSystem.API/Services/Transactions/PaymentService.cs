using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Middleware;
using EventParkingReservationSystem.API.Models.Transactions;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Transactions;

public sealed class PaymentService(AppDbContext db) : IPaymentService
{
    public async Task<PaymentDto?> GetForBookingAsync(int bookingId, CancellationToken ct) =>
        (await db.Payments.AsNoTracking().SingleOrDefaultAsync(x => x.BookingId == bookingId, ct)) is { } p ? Map(p) : null;

    public async Task<PaymentDto> StartAsync(int bookingId, PaymentRequestDto request, CancellationToken ct)
    {
        var booking = await db.Bookings.SingleOrDefaultAsync(x => x.Id == bookingId, ct) ?? throw new NotFoundException("Booking not found.");
        if (await db.Payments.AnyAsync(x => x.BookingId == bookingId, ct)) throw new ConflictException("Payment already exists for this booking.");
        var payment = new Payment { BookingId = bookingId, Amount = booking.TotalAmount, Method = request.Method.Trim(), TransactionReference = $"TXN-{Guid.NewGuid():N}".ToUpperInvariant() };
        db.Payments.Add(payment);
        await db.SaveChangesAsync(ct);
        return Map(payment);
    }

    public async Task<PaymentDto> CompleteAsync(int paymentId, CancellationToken ct)
    {
        var payment = await db.Payments.Include(x => x.Booking).SingleOrDefaultAsync(x => x.Id == paymentId, ct) ?? throw new NotFoundException("Payment not found.");
        if (payment.Status == PaymentStatus.Completed) throw new ConflictException("Payment is already completed.");
        payment.Status = PaymentStatus.Completed;
        payment.CompletedAtUtc = DateTime.UtcNow;
        payment.Booking.Status = BookingStatus.Confirmed;
        var token = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24));
        db.QrCodes.Add(new QrCode { BookingId = payment.BookingId, Token = token, Payload = $"BOOKING:{payment.Booking.BookingNumber}|TOKEN:{token}" });
        db.Notifications.Add(new Notification { CustomerId = payment.Booking.CustomerId, BookingId = payment.BookingId, Title = "Booking confirmed", Message = $"Payment received. Booking {payment.Booking.BookingNumber} is confirmed." });
        await db.SaveChangesAsync(ct);
        return Map(payment);
    }

    public async Task<IReadOnlyList<PaymentDto>> GetCustomerHistoryAsync(int customerId, CancellationToken ct) =>
        (await db.Payments.AsNoTracking().Where(x => x.Booking.CustomerId == customerId).OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct)).Select(Map).ToList();

    public async Task<PaymentReceiptDto> GetReceiptAsync(int paymentId, CancellationToken ct)
    {
        var p = await db.Payments.AsNoTracking().Include(x => x.Booking).SingleOrDefaultAsync(x => x.Id == paymentId && x.Status == PaymentStatus.Completed, ct)
            ?? throw new NotFoundException("Completed payment receipt not found.");
        return new($"RCT-{p.Id:000000}", p.Booking.BookingNumber, p.Amount, p.Method, p.TransactionReference, p.CompletedAtUtc!.Value);
    }

    private static PaymentDto Map(Payment p) => new(p.Id, p.BookingId, p.Amount, p.Method, p.Status.ToString(), p.TransactionReference, p.CreatedAtUtc, p.CompletedAtUtc);
}
