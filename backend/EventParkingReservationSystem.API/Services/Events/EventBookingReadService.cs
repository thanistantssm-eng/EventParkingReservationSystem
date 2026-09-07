using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Interfaces.Events;
using EventParkingReservationSystem.API.Models.Transactions;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Events;

/// <summary>
/// Member 3 booking data exposed through Member 2's
/// cross-module read contract.
/// </summary>
public sealed class EventBookingReadService(
    AppDbContext db) : IEventBookingReadService
{
    public Task<bool> HasActiveBookingsAsync(
        int eventId,
        CancellationToken cancellationToken = default) =>
        db.Bookings
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.EventId == eventId &&
                    x.Status !=
                        BookingStatus.Cancelled,
                cancellationToken);

    public async Task<IReadOnlyCollection<int>>
        GetBookedSeatIdsAsync(
            int eventId,
            CancellationToken cancellationToken = default) =>
        await db.BookingSeats
            .AsNoTracking()
            .Where(
                x =>
                    x.Booking.EventId ==
                        eventId &&
                    x.Booking.Status !=
                        BookingStatus.Cancelled)
            .Select(x => x.SeatId)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<int>>
        GetOccupiedParkingSlotIdsAsync(
            int eventId,
            CancellationToken cancellationToken = default) =>
        await db.BookingParkings
            .AsNoTracking()
            .Where(
                x =>
                    x.Booking.EventId ==
                        eventId &&
                    x.Booking.Status !=
                        BookingStatus.Cancelled)
            .Select(x => x.ParkingSlotId)
            .Distinct()
            .ToListAsync(cancellationToken);
}
