namespace EventParkingReservationSystem.API.Interfaces.Events;

/// <summary>
/// Cross-module read contract implemented by Member 3 after Booking/Transaction code is merged.
/// Member 2 uses it without editing Member 3 files.
/// </summary>
public interface IEventBookingReadService
{
    Task<bool> HasActiveBookingsAsync(int eventId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<int>> GetBookedSeatIdsAsync(
        int eventId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<int>> GetOccupiedParkingSlotIdsAsync(
        int eventId,
        CancellationToken cancellationToken = default);
}
