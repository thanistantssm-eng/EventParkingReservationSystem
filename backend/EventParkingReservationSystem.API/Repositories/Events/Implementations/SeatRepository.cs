using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Repositories.Events.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Repositories.Events.Implementations;

public class SeatRepository(AppDbContext db) : ISeatRepository
{
    private readonly AppDbContext _db = db;

    public IQueryable<Seat> Query(bool tracking = false)
    {
        var query = _db.Set<Seat>().AsQueryable();
        return tracking ? query : query.AsNoTracking();
    }

    public Task<Seat?> GetByIdAsync(int id, bool tracking = true, CancellationToken cancellationToken = default)
    {
        var query = _db.Set<Seat>().AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task AddAsync(Seat entity, CancellationToken cancellationToken = default) =>
        _db.Set<Seat>().AddAsync(entity, cancellationToken).AsTask();

    public void Remove(Seat entity) => _db.Set<Seat>().Remove(entity);

    public Task<bool> SeatNumberExistsAsync(int eventId, string seatNumber, int? excludeId = null, CancellationToken cancellationToken = default) =>
        _db.Set<Seat>().AnyAsync(x =>
            x.EventId == eventId && x.SeatNumber == seatNumber && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}
