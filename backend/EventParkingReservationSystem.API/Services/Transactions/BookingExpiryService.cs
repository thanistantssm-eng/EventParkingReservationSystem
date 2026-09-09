using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Models.Transactions;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Transactions;

public sealed class BookingExpiryService(
    AppDbContext db,
    IConfiguration configuration) : IBookingExpiryService
{
    private int HoldMinutes => Math.Clamp(
        configuration.GetValue<int?>("BookingHold:Minutes") ?? 15,
        1,
        120);

    public async Task ExpireStalePendingBookingsAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddMinutes(-HoldMinutes);

        var staleBookings = await db.Bookings
            .Include(x => x.Seats)
            .Include(x => x.Parking)
            .Include(x => x.Payment)
            .Where(x =>
                x.Status == BookingStatus.PendingPayment &&
                x.CreatedAtUtc <= cutoff)
            .ToListAsync(ct);

        if (staleBookings.Count == 0)
        {
            return;
        }

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

            if (booking.Payment is not null &&
                booking.Payment.Status == PaymentStatus.PendingOtp)
            {
                booking.Payment.Status = PaymentStatus.Failed;
            }

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAtUtc = now;

            db.Notifications.Add(new Notification
            {
                CustomerId = booking.CustomerId,
                BookingId = booking.Id,
                Title = "Booking expired",
                Message = $"Booking {booking.BookingNumber} expired because payment was not completed within {HoldMinutes} minutes."
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
