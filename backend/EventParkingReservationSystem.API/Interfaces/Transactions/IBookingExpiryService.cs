namespace EventParkingReservationSystem.API.Interfaces.Transactions;

public interface IBookingExpiryService
{
    Task ExpireStalePendingBookingsAsync(CancellationToken ct);
}
