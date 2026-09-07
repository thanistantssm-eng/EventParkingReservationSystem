using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Repositories.Events.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Repositories.Events.Implementations;

public class TicketRepository(AppDbContext db) : ITicketRepository
{
    private readonly AppDbContext _db = db;

    public IQueryable<TicketType> Query(bool tracking = false)
    {
        var query = _db.Set<TicketType>().AsQueryable();
        return tracking ? query : query.AsNoTracking();
    }

    public Task<TicketType?> GetByIdAsync(int id, bool tracking = true, CancellationToken cancellationToken = default)
    {
        var query = _db.Set<TicketType>().AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task AddAsync(TicketType entity, CancellationToken cancellationToken = default) =>
        _db.Set<TicketType>().AddAsync(entity, cancellationToken).AsTask();

    public void Remove(TicketType entity) => _db.Set<TicketType>().Remove(entity);

    public Task<bool> NameExistsAsync(int eventId, string name, int? excludeId = null, CancellationToken cancellationToken = default) =>
        _db.Set<TicketType>().AnyAsync(x =>
            x.EventId == eventId && x.Name == name && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}
