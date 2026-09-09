using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Models.Transactions;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Transactions;

public sealed class BookingExpiryService(
    AppDbContext db,
    IConfiguration configuration,
    IEmailService emailService,
    ILogger<BookingExpiryService> logger) : IBookingExpiryService
{
    // Prevent request-triggered cleanup and the hosted background worker
    // from expiring the same booking at the same time in this app instance.
    private static readonly SemaphoreSlim CleanupLock = new(1, 1);

    private int HoldMinutes => Math.Clamp(
        configuration.GetValue<int?>("BookingHold:Minutes") ?? 15,
        1,
        120);

    public async Task ExpireStalePendingBookingsAsync(CancellationToken ct)
    {
        await CleanupLock.WaitAsync(ct);

        try
        {
            var now = DateTime.UtcNow;
            var cutoff = now.AddMinutes(-HoldMinutes);

            var staleBookings = await db.Bookings
                .Include(x => x.Seats)
                .Include(x => x.Parking)
                .Include(x => x.Payment)
                .Include(x => x.Customer)
                .Where(x =>
                    x.Status == BookingStatus.PendingPayment &&
                    x.CreatedAtUtc <= cutoff)
                .ToListAsync(ct);

            if (staleBookings.Count == 0)
            {
                return;
            }

            var expiryEmails = new List<ExpiryEmail>();
            var expiredPaymentIds = new List<int>();

            foreach (var booking in staleBookings)
            {
                if (booking.Payment?.Status == PaymentStatus.Completed)
                {
                    // Defensive guard: a completed payment must never be auto-expired.
                    booking.Status = BookingStatus.Confirmed;
                    continue;
                }

                if (booking.Seats.Count > 0)
                {
                    db.BookingSeats.RemoveRange(booking.Seats);
                }

                if (booking.Parking is not null)
                {
                    db.BookingParkings.Remove(booking.Parking);
                }

                if (booking.Payment is not null)
                {
                    expiredPaymentIds.Add(booking.Payment.Id);

                    if (booking.Payment.Status == PaymentStatus.PendingOtp)
                    {
                        booking.Payment.Status = PaymentStatus.Failed;
                    }
                }

                booking.Status = BookingStatus.Cancelled;
                booking.CancelledAtUtc = now;

                db.Notifications.Add(new Notification
                {
                    CustomerId = booking.CustomerId,
                    BookingId = booking.Id,
                    Title = "Booking expired",
                    Message =
                        $"Booking {booking.BookingNumber} was automatically cancelled because payment was not completed within {HoldMinutes} minutes. Your reserved seats and parking have been released."
                });

                if (!string.IsNullOrWhiteSpace(booking.Customer.Email))
                {
                    expiryEmails.Add(new ExpiryEmail(
                        booking.Customer.Email,
                        booking.Customer.Name,
                        booking.BookingNumber));
                }
            }

            // Any OTP issued for a payment that just expired is no longer useful.
            if (expiredPaymentIds.Count > 0)
            {
                var staleOtps = await db.OtpVerifications
                    .Where(x => expiredPaymentIds.Contains(x.PaymentId))
                    .ToListAsync(ct);

                if (staleOtps.Count > 0)
                {
                    db.OtpVerifications.RemoveRange(staleOtps);
                }
            }

            // Persist the cancellation/release first. Email delivery must never
            // keep inventory locked when SMTP is unavailable.
            await db.SaveChangesAsync(ct);

            foreach (var email in expiryEmails)
            {
                try
                {
                    await emailService.SendBookingExpiredAsync(
                        email.Email,
                        email.CustomerName,
                        email.BookingNumber,
                        HoldMinutes);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(
                        ex,
                        "Booking {BookingNumber} expired successfully, but the expiry email could not be sent to {Email}.",
                        email.BookingNumber,
                        email.Email);
                }
            }
        }
        finally
        {
            CleanupLock.Release();
        }
    }

    private sealed record ExpiryEmail(
        string Email,
        string CustomerName,
        string BookingNumber);
}
