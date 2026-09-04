using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Repositories.Events.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Repositories.Events.Implementations;

public class ApprovalRepository(AppDbContext db) : IApprovalRepository
{
    private readonly AppDbContext _db = db;

    public IQueryable<EventApproval> Query(bool tracking = false)
    {
        var query = _db.Set<EventApproval>().AsQueryable();
        return tracking ? query : query.AsNoTracking();
    }

    public Task<EventApproval?> GetByIdAsync(int id, bool tracking = true, CancellationToken cancellationToken = default)
    {
        var query = _db.Set<EventApproval>().AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<EventApproval?> GetPendingForEventAsync(int eventId, CancellationToken cancellationToken = default) =>
        _db.Set<EventApproval>().FirstOrDefaultAsync(x => x.EventId == eventId && x.Status == ApprovalStatus.Pending, cancellationToken);

    public Task AddAsync(EventApproval entity, CancellationToken cancellationToken = default) =>
        _db.Set<EventApproval>().AddAsync(entity, cancellationToken).AsTask();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}
