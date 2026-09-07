using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Repositories.Events.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Repositories.Events.Implementations;

public class EventRepository(AppDbContext db) : IEventRepository
{
    private readonly AppDbContext _db = db;

    public IQueryable<Event> Query(bool tracking = false)
    {
        var query = _db.Set<Event>().AsQueryable();
        return tracking ? query : query.AsNoTracking();
    }

    public Task<Event?> GetByIdAsync(int id, bool tracking = true, CancellationToken cancellationToken = default)
    {
        var query = _db.Set<Event>().AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task AddAsync(Event entity, CancellationToken cancellationToken = default) =>
        _db.Set<Event>().AddAsync(entity, cancellationToken).AsTask();

    public void Remove(Event entity) => _db.Set<Event>().Remove(entity);

    public Task<bool> HasVenueTimeConflictAsync(
        int venueId,
        DateTime start,
        DateTime end,
        int? excludeEventId = null,
        CancellationToken cancellationToken = default) =>
        _db.Set<Event>().AnyAsync(x =>
            x.VenueId == venueId &&
            x.Status != EventStatus.Cancelled &&
            (!excludeEventId.HasValue || x.Id != excludeEventId.Value) &&
            start < x.EndDateTime &&
            end > x.StartDateTime,
            cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
