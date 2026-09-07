using EventParkingReservationSystem.API.Models.Events;

namespace EventParkingReservationSystem.API.Repositories.Events.Interfaces;

public interface IParkingRepository
{
    IQueryable<ParkingArea> Areas(bool tracking = false);
    IQueryable<ParkingSlot> Slots(bool tracking = false);
    IQueryable<EventParkingAllocation> Allocations(bool tracking = false);

    Task<ParkingArea?> GetAreaAsync(int id, bool tracking = true, CancellationToken cancellationToken = default);
    Task<ParkingSlot?> GetSlotAsync(int id, bool tracking = true, CancellationToken cancellationToken = default);
    Task<EventParkingAllocation?> GetAllocationAsync(int id, bool tracking = true, CancellationToken cancellationToken = default);

    Task AddAreaAsync(ParkingArea entity, CancellationToken cancellationToken = default);
    Task AddSlotAsync(ParkingSlot entity, CancellationToken cancellationToken = default);
    Task AddAllocationAsync(EventParkingAllocation entity, CancellationToken cancellationToken = default);

    void RemoveSlot(ParkingSlot entity);
    void RemoveAllocation(EventParkingAllocation entity);

    Task<bool> AreaNameExistsAsync(int venueId, string name, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> SlotNumberExistsAsync(int areaId, string slotNumber, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> AllocationExistsAsync(int eventId, int areaId, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
