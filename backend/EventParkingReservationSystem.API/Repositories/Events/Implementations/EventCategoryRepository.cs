using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Repositories.Events.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Repositories.Events.Implementations;

public class EventCategoryRepository(AppDbContext db) : IEventCategoryRepository
{
    private readonly AppDbContext _db = db;

    public IQueryable<EventCategory> Query(bool tracking = false)
    {
        var query = _db.Set<EventCategory>().AsQueryable();
        return tracking ? query : query.AsNoTracking();
    }

    public Task<EventCategory?> GetByIdAsync(int id, bool tracking = true, CancellationToken cancellationToken = default)
    {
        var query = _db.Set<EventCategory>().AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task AddAsync(EventCategory entity, CancellationToken cancellationToken = default) =>
        _db.Set<EventCategory>().AddAsync(entity, cancellationToken).AsTask();

    public Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default) =>
        _db.Set<EventCategory>().AnyAsync(x =>
            x.Name == name && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

    public Task<bool> IsUsedByAnyEventAsync(int categoryId, CancellationToken cancellationToken = default) =>
        _db.Set<Event>().AnyAsync(x => x.EventCategoryId == categoryId && x.Status != EventStatus.Cancelled, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}
