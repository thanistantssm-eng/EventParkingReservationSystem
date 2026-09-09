using System.Security.Cryptography;
using System.Text;
using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Middleware;
using EventParkingReservationSystem.API.Models.Transactions;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Transactions;

public sealed class OtpService(
    AppDbContext db,
    IPaymentService payments,
    IBookingExpiryService expiry,
    IEmailService emailService,
    IHostEnvironment environment,
    IConfiguration configuration) : IOtpService
{
    public async Task<OtpIssuedDto> IssueAsync(
        int paymentId,
        int customerId,
        CancellationToken ct)
    {
        await expiry.ExpireStalePendingBookingsAsync(ct);

        var payment = await db.Payments
            .Include(x => x.Booking)
                .ThenInclude(x => x.Customer)
            .SingleOrDefaultAsync(x => x.Id == paymentId, ct)
            ?? throw new NotFoundException("Payment not found.");

        if (payment.Booking.CustomerId != customerId)
        {
            throw new ApiException(
                403,
                "You can only request an OTP for your own payment.");
        }

        if (payment.Booking.Status != BookingStatus.PendingPayment ||
            payment.Status != PaymentStatus.PendingOtp)
        {
            throw new ConflictException(
                "OTP can only be requested for an active pending payment.");
        }

        var existing = await db.OtpVerifications
            .SingleOrDefaultAsync(x => x.PaymentId == paymentId, ct);

        if (existing is not null)
        {
            db.OtpVerifications.Remove(existing);
        }

        var code = RandomNumberGenerator
            .GetInt32(0, 1_000_000)
            .ToString("D6");

        var expires = DateTime.UtcNow.AddMinutes(5);

        db.OtpVerifications.Add(new OtpVerification
        {
            PaymentId = paymentId,
            CodeHash = Hash(code),
            ExpiresAtUtc = expires
        });

        await db.SaveChangesAsync(ct);

        await emailService.SendPaymentOtpAsync(
            payment.Booking.Customer.Email,
            payment.Booking.Customer.Name,
            code,
            payment.Booking.BookingNumber);

        var exposeDevelopmentCode =
            environment.IsDevelopment() &&
            configuration.GetValue<bool>("Otp:ExposeDevelopmentCode");

        return new(
            paymentId,
            expires,
            exposeDevelopmentCode ? code : null);
    }

    public async Task<PaymentDto> VerifyAsync(
        OtpVerifyDto request,
        int customerId,
        CancellationToken ct)
    {
        await expiry.ExpireStalePendingBookingsAsync(ct);

        var payment = await db.Payments
            .AsNoTracking()
            .Include(x => x.Booking)
            .SingleOrDefaultAsync(x => x.Id == request.PaymentId, ct)
            ?? throw new NotFoundException("Payment not found.");

        if (payment.Booking.CustomerId != customerId)
        {
            throw new ApiException(
                403,
                "You can only verify an OTP for your own payment.");
        }

        if (payment.Booking.Status != BookingStatus.PendingPayment ||
            payment.Status != PaymentStatus.PendingOtp)
        {
            throw new ConflictException(
                "This payment is no longer active or the booking has expired/cancelled.");
        }

        var otp = await db.OtpVerifications
            .SingleOrDefaultAsync(x => x.PaymentId == request.PaymentId, ct)
            ?? throw new NotFoundException("OTP request not found.");

        if (otp.VerifiedAtUtc.HasValue)
        {
            throw new ConflictException("OTP has already been used.");
        }

        if (otp.ExpiresAtUtc < DateTime.UtcNow)
        {
            throw new ValidationException("OTP has expired.");
        }

        if (otp.FailedAttempts >= 5)
        {
            throw new ApiException(429, "Too many failed OTP attempts.");
        }

        if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(otp.CodeHash),
                Convert.FromHexString(Hash(request.Code))))
        {
            otp.FailedAttempts++;
            await db.SaveChangesAsync(ct);
            throw new ValidationException("Invalid OTP.");
        }

        otp.VerifiedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return await payments.CompleteAsync(request.PaymentId, ct);
    }

    private static string Hash(string value) =>
        Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(value)));
}
