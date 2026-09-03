using EventParkingReservationSystem.API.DTOs.Events;

namespace EventParkingReservationSystem.API.Interfaces.Events;

public interface IParkingService
{
    Task<IReadOnlyList<ParkingAreaDto>> GetAreasAsync(int? venueId = null, CancellationToken cancellationToken = default);
    Task<ParkingAreaDto> CreateAreaAsync(CreateParkingAreaDto dto, CancellationToken cancellationToken = default);
    Task<ParkingAreaDto> UpdateAreaAsync(int areaId, UpdateParkingAreaDto dto, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ParkingSlotDto>> GetSlotsAsync(int areaId, CancellationToken cancellationToken = default);
    Task<ParkingSlotDto> CreateSlotAsync(int areaId, CreateParkingSlotDto dto, CancellationToken cancellationToken = default);
    Task<ParkingSlotDto> UpdateSlotAsync(int slotId, UpdateParkingSlotDto dto, CancellationToken cancellationToken = default);
    Task DeleteSlotAsync(int slotId, CancellationToken cancellationToken = default);

    Task<ParkingLayoutDto> GetEventLayoutAsync(int eventId, int? actorOrganizerId, string? actorRole, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingSlotDto>> GetEventParkingSlotsAsync(int eventId, int? actorOrganizerId, string? actorRole, CancellationToken cancellationToken = default);
    Task<EventParkingAllocationDto> AllocateAreaAsync(int eventId, CreateEventParkingAllocationDto dto, int? actorOrganizerId, string actorRole, CancellationToken cancellationToken = default);
    Task DeleteAllocationAsync(int allocationId, int? actorOrganizerId, string actorRole, CancellationToken cancellationToken = default);
}
