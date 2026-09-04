using EventParkingReservationSystem.API.Models.Events;

namespace EventParkingReservationSystem.API.Repositories.Events.Interfaces;

public interface IEventCategoryRepository
{
    IQueryable<EventCategory> Query(bool tracking = false);
    Task<EventCategory?> GetByIdAsync(int id, bool tracking = true, CancellationToken cancellationToken = default);
    Task AddAsync(EventCategory entity, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> IsUsedByAnyEventAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
