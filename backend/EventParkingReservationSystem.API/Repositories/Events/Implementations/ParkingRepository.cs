using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Repositories.Events.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Repositories.Events.Implementations;

public class ParkingRepository(AppDbContext db) : IParkingRepository
{
    private readonly AppDbContext _db = db;

    public IQueryable<ParkingArea> Areas(bool tracking = false)
    {
        var query = _db.Set<ParkingArea>().AsQueryable();
        return tracking ? query : query.AsNoTracking();
    }

    public IQueryable<ParkingSlot> Slots(bool tracking = false)
    {
        var query = _db.Set<ParkingSlot>().AsQueryable();
        return tracking ? query : query.AsNoTracking();
    }

    public IQueryable<EventParkingAllocation> Allocations(bool tracking = false)
    {
        var query = _db.Set<EventParkingAllocation>().AsQueryable();
        return tracking ? query : query.AsNoTracking();
    }

    public Task<ParkingArea?> GetAreaAsync(int id, bool tracking = true, CancellationToken cancellationToken = default)
    {
        var query = _db.Set<ParkingArea>().AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<ParkingSlot?> GetSlotAsync(int id, bool tracking = true, CancellationToken cancellationToken = default)
    {
        var query = _db.Set<ParkingSlot>().AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<EventParkingAllocation?> GetAllocationAsync(int id, bool tracking = true, CancellationToken cancellationToken = default)
    {
        var query = _db.Set<EventParkingAllocation>().AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task AddAreaAsync(ParkingArea entity, CancellationToken cancellationToken = default) =>
        _db.Set<ParkingArea>().AddAsync(entity, cancellationToken).AsTask();

    public Task AddSlotAsync(ParkingSlot entity, CancellationToken cancellationToken = default) =>
        _db.Set<ParkingSlot>().AddAsync(entity, cancellationToken).AsTask();

    public Task AddAllocationAsync(EventParkingAllocation entity, CancellationToken cancellationToken = default) =>
        _db.Set<EventParkingAllocation>().AddAsync(entity, cancellationToken).AsTask();

    public void RemoveSlot(ParkingSlot entity) => _db.Set<ParkingSlot>().Remove(entity);
    public void RemoveAllocation(EventParkingAllocation entity) => _db.Set<EventParkingAllocation>().Remove(entity);

    public Task<bool> AreaNameExistsAsync(int venueId, string name, int? excludeId = null, CancellationToken cancellationToken = default) =>
        _db.Set<ParkingArea>().AnyAsync(
            x => x.VenueId == venueId &&
                 x.Name == name &&
                 (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);

    public Task<bool> SlotNumberExistsAsync(int areaId, string slotNumber, int? excludeId = null, CancellationToken cancellationToken = default) =>
        _db.Set<ParkingSlot>().AnyAsync(
            x => x.ParkingAreaId == areaId &&
                 x.SlotNumber == slotNumber &&
                 (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);

    public Task<bool> AllocationExistsAsync(int eventId, int areaId, int? excludeId = null, CancellationToken cancellationToken = default) =>
        _db.Set<EventParkingAllocation>().AnyAsync(
            x => x.EventId == eventId &&
                 x.ParkingAreaId == areaId &&
                 (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
