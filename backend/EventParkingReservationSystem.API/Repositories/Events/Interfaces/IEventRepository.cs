using EventParkingReservationSystem.API.Models.Events;

namespace EventParkingReservationSystem.API.Repositories.Events.Interfaces;

public interface IEventRepository
{
    IQueryable<Event> Query(bool tracking = false);
    Task<Event?> GetByIdAsync(int id, bool tracking = true, CancellationToken cancellationToken = default);
    Task AddAsync(Event entity, CancellationToken cancellationToken = default);
    void Remove(Event entity);
    Task<bool> HasVenueTimeConflictAsync(int venueId, DateTime start, DateTime end, int? excludeEventId = null, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
