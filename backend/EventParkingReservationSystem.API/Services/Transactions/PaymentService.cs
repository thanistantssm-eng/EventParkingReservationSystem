using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Middleware;
using EventParkingReservationSystem.API.Models.Transactions;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Transactions;

public sealed class PaymentService(
    AppDbContext db,
    IBookingExpiryService expiry) : IPaymentService
{
    public async Task<PaymentDto?> GetForBookingAsync(
        int bookingId,
        CancellationToken ct)
    {
        await expiry.ExpireStalePendingBookingsAsync(ct);

        var payment = await db.Payments
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.BookingId == bookingId, ct);

        return payment is null ? null : Map(payment);
    }

    public async Task<PaymentDto> StartAsync(
        int bookingId,
        PaymentRequestDto request,
        CancellationToken ct)
    {
        await expiry.ExpireStalePendingBookingsAsync(ct);

        var booking = await db.Bookings
            .SingleOrDefaultAsync(x => x.Id == bookingId, ct)
            ?? throw new NotFoundException("Booking not found.");

        if (booking.Status != BookingStatus.PendingPayment)
        {
            throw new ConflictException(
                "Payment can only be started for an active pending-payment booking.");
        }

        if (await db.Payments.AnyAsync(x => x.BookingId == bookingId, ct))
        {
            throw new ConflictException("Payment already exists for this booking.");
        }

        var method = request.Method.Trim();
        if (string.IsNullOrWhiteSpace(method))
        {
            throw new ValidationException("Payment method is required.");
        }

        var payment = new Payment
        {
            BookingId = bookingId,
            Amount = booking.TotalAmount,
            Method = method,
            TransactionReference =
                $"TXN-{Guid.NewGuid():N}".ToUpperInvariant()
        };

        db.Payments.Add(payment);
        await db.SaveChangesAsync(ct);
        return Map(payment);
    }

    public async Task<PaymentDto> CompleteAsync(
        int paymentId,
        CancellationToken ct)
    {
        await expiry.ExpireStalePendingBookingsAsync(ct);

        var payment = await db.Payments
            .Include(x => x.Booking)
            .ThenInclude(x => x.QrCode)
            .SingleOrDefaultAsync(x => x.Id == paymentId, ct)
            ?? throw new NotFoundException("Payment not found.");

        if (payment.Status == PaymentStatus.Completed)
        {
            throw new ConflictException("Payment is already completed.");
        }

        if (payment.Status != PaymentStatus.PendingOtp ||
            payment.Booking.Status != BookingStatus.PendingPayment)
        {
            throw new ConflictException(
                "This payment is no longer active or the booking has expired/cancelled.");
        }

        payment.Status = PaymentStatus.Completed;
        payment.CompletedAtUtc = DateTime.UtcNow;
        payment.Booking.Status = BookingStatus.Confirmed;

        if (payment.Booking.QrCode is null)
        {
            var token = Convert.ToHexString(
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(24));

            db.QrCodes.Add(new QrCode
            {
                BookingId = payment.BookingId,
                Token = token,
                Payload =
                    $"BOOKING:{payment.Booking.BookingNumber}|TOKEN:{token}"
            });
        }

        db.Notifications.Add(new Notification
        {
            CustomerId = payment.Booking.CustomerId,
            BookingId = payment.BookingId,
            Title = "Booking confirmed",
            Message =
                $"Payment received. Booking {payment.Booking.BookingNumber} is confirmed."
        });

        await db.SaveChangesAsync(ct);
        return Map(payment);
    }

    public async Task<PaymentDto> RefundAsync(
        int paymentId,
        CancellationToken ct)
    {
        var payment = await db.Payments
            .Include(x => x.Booking)
                .ThenInclude(x => x.Seats)
            .Include(x => x.Booking)
                .ThenInclude(x => x.Parking)
            .Include(x => x.Booking)
                .ThenInclude(x => x.QrCode)
            .SingleOrDefaultAsync(x => x.Id == paymentId, ct)
            ?? throw new NotFoundException("Payment not found.");

        if (payment.Status == PaymentStatus.Refunded)
        {
            throw new ConflictException("Payment is already refunded.");
        }

        if (payment.Status != PaymentStatus.Completed)
        {
            throw new ConflictException("Only completed payments can be refunded.");
        }

        payment.Status = PaymentStatus.Refunded;

        if (payment.Booking.Seats.Count > 0)
        {
            db.BookingSeats.RemoveRange(payment.Booking.Seats);
        }

        if (payment.Booking.Parking is not null)
        {
            db.BookingParkings.Remove(payment.Booking.Parking);
        }

        if (payment.Booking.QrCode is not null)
        {
            db.QrCodes.Remove(payment.Booking.QrCode);
        }

        payment.Booking.Status = BookingStatus.Cancelled;
        payment.Booking.CancelledAtUtc = DateTime.UtcNow;

        db.Notifications.Add(new Notification
        {
            CustomerId = payment.Booking.CustomerId,
            BookingId = payment.BookingId,
            Title = "Payment refunded",
            Message =
                $"Payment for booking {payment.Booking.BookingNumber} was refunded and the booking was cancelled."
        });

        await db.SaveChangesAsync(ct);
        return Map(payment);
    }

    public async Task<IReadOnlyList<PaymentDto>> GetCustomerHistoryAsync(
        int customerId,
        CancellationToken ct)
    {
        await expiry.ExpireStalePendingBookingsAsync(ct);

        return (await db.Payments
            .AsNoTracking()
            .Where(x => x.Booking.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct))
            .Select(Map)
            .ToList();
    }

    public async Task<PaymentReceiptDto> GetReceiptAsync(
        int paymentId,
        CancellationToken ct)
    {
        var p = await db.Payments
            .AsNoTracking()
            .Include(x => x.Booking)
            .SingleOrDefaultAsync(
                x => x.Id == paymentId &&
                     (x.Status == PaymentStatus.Completed ||
                      x.Status == PaymentStatus.Refunded),
                ct)
            ?? throw new NotFoundException("Completed payment receipt not found.");

        return new(
            $"RCT-{p.Id:000000}",
            p.Booking.BookingNumber,
            p.Amount,
            p.Method,
            p.TransactionReference,
            p.CompletedAtUtc!.Value);
    }

    private static PaymentDto Map(Payment p) =>
        new(
            p.Id,
            p.BookingId,
            p.Amount,
            p.Method,
            p.Status.ToString(),
            p.TransactionReference,
            p.CreatedAtUtc,
            p.CompletedAtUtc);
}
