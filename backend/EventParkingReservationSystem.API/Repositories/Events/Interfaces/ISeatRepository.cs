using EventParkingReservationSystem.API.Models.Events;

namespace EventParkingReservationSystem.API.Repositories.Events.Interfaces;

public interface ISeatRepository
{
    IQueryable<Seat> Query(bool tracking = false);
    Task<Seat?> GetByIdAsync(int id, bool tracking = true, CancellationToken cancellationToken = default);
    Task AddAsync(Seat entity, CancellationToken cancellationToken = default);
    void Remove(Seat entity);
    Task<bool> SeatNumberExistsAsync(int eventId, string seatNumber, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
