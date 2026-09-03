using EventParkingReservationSystem.API.DTOs.Events;
using EventParkingReservationSystem.API.Interfaces.Events;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Repositories.Events.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Events;

public class ParkingService(
    IParkingRepository parkingRepository,
    IEventRepository eventRepository,
    IEventBookingReadService? bookingReadService = null,
    IEventReferenceReadService? referenceReadService = null) : IParkingService
{
    private readonly IParkingRepository _parking = parkingRepository;
    private readonly IEventRepository _events = eventRepository;
    private readonly IEventBookingReadService? _bookings = bookingReadService;
    private readonly IEventReferenceReadService? _references = referenceReadService;

    public async Task<IReadOnlyList<ParkingAreaDto>> GetAreasAsync(
        int? venueId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _parking.Areas();

        if (venueId.HasValue)
            query = query.Where(x => x.VenueId == venueId.Value);

        var list = await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return list.Select(MapArea).ToList();
    }

    public async Task<ParkingAreaDto> CreateAreaAsync(
        CreateParkingAreaDto dto,
        CancellationToken cancellationToken = default)
    {
        if (_references is not null &&
            !await _references.VenueExistsAsync(dto.VenueId, cancellationToken))
        {
            throw new ArgumentException("Venue does not exist.");
        }

        var name = dto.Name.Trim();

        if (await _parking.AreaNameExistsAsync(
                dto.VenueId,
                name,
                null,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "A parking area with this name already exists for the venue.");
        }

        var entity = new ParkingArea
        {
            VenueId = dto.VenueId,
            Name = name,
            Description = Normalize(dto.Description),
            Capacity = dto.Capacity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _parking.AddAreaAsync(entity, cancellationToken);
        await _parking.SaveChangesAsync(cancellationToken);

        return MapArea(entity);
    }

    public async Task<ParkingAreaDto> UpdateAreaAsync(
        int areaId,
        UpdateParkingAreaDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _parking.GetAreaAsync(
            areaId,
            true,
            cancellationToken)
            ?? throw new KeyNotFoundException("Parking area not found.");

        await EnsureParkingAreaCanChangeAsync(areaId, cancellationToken);

        if (_references is not null &&
            !await _references.VenueExistsAsync(dto.VenueId, cancellationToken))
        {
            throw new ArgumentException("Venue does not exist.");
        }

        var name = dto.Name.Trim();

        if (await _parking.AreaNameExistsAsync(
                dto.VenueId,
                name,
                areaId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "A parking area with this name already exists for the venue.");
        }

        entity.VenueId = dto.VenueId;
        entity.Name = name;
        entity.Description = Normalize(dto.Description);
        entity.Capacity = dto.Capacity;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _parking.SaveChangesAsync(cancellationToken);

        return MapArea(entity);
    }

    public async Task<IReadOnlyList<ParkingSlotDto>> GetSlotsAsync(
        int areaId,
        CancellationToken cancellationToken = default)
    {
        if (await _parking.GetAreaAsync(
                areaId,
                false,
                cancellationToken) is null)
        {
            throw new KeyNotFoundException("Parking area not found.");
        }

        var list = await _parking.Slots()
            .Where(x => x.ParkingAreaId == areaId)
            .OrderBy(x => x.SlotNumber)
            .ToListAsync(cancellationToken);

        return list
            .Select(x => MapSlot(x, false, 0m))
            .ToList();
    }

    public async Task<ParkingSlotDto> CreateSlotAsync(
        int areaId,
        CreateParkingSlotDto dto,
        CancellationToken cancellationToken = default)
    {
        var area = await _parking.GetAreaAsync(
            areaId,
            false,
            cancellationToken)
            ?? throw new KeyNotFoundException("Parking area not found.");

        var currentCount = await _parking.Slots()
            .CountAsync(
                x => x.ParkingAreaId == areaId && x.IsActive,
                cancellationToken);

        if (currentCount >= area.Capacity)
        {
            throw new InvalidOperationException(
                "Parking area capacity has been reached.");
        }

        var number = dto.SlotNumber.Trim();

        if (await _parking.SlotNumberExistsAsync(
                areaId,
                number,
                null,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "This parking slot number already exists in the area.");
        }

        var entity = new ParkingSlot
        {
            ParkingAreaId = areaId,
            SlotNumber = number,
            SlotType = Normalize(dto.SlotType),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _parking.AddSlotAsync(entity, cancellationToken);
        await _parking.SaveChangesAsync(cancellationToken);

        return MapSlot(entity, false, 0m);
    }

    public async Task<ParkingSlotDto> UpdateSlotAsync(
        int slotId,
        UpdateParkingSlotDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _parking.GetSlotAsync(
            slotId,
            true,
            cancellationToken)
            ?? throw new KeyNotFoundException("Parking slot not found.");

        await EnsureParkingAreaCanChangeAsync(
            entity.ParkingAreaId,
            cancellationToken);

        var number = dto.SlotNumber.Trim();

        if (await _parking.SlotNumberExistsAsync(
                entity.ParkingAreaId,
                number,
                slotId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "This parking slot number already exists in the area.");
        }

        entity.SlotNumber = number;
        entity.SlotType = Normalize(dto.SlotType);
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _parking.SaveChangesAsync(cancellationToken);

        return MapSlot(entity, false, 0m);
    }

    public async Task DeleteSlotAsync(
        int slotId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _parking.GetSlotAsync(
            slotId,
            true,
            cancellationToken)
            ?? throw new KeyNotFoundException("Parking slot not found.");

        await EnsureParkingAreaCanChangeAsync(
            entity.ParkingAreaId,
            cancellationToken);

        _parking.RemoveSlot(entity);
        await _parking.SaveChangesAsync(cancellationToken);
    }

    public async Task<ParkingLayoutDto> GetEventLayoutAsync(
        int eventId,
        int? actorOrganizerId,
        string? actorRole,
        CancellationToken cancellationToken = default)
    {
        var evt = await _events.GetByIdAsync(
            eventId,
            false,
            cancellationToken)
            ?? throw new KeyNotFoundException("Event not found.");

        EnsureCanViewEvent(evt, actorOrganizerId, actorRole);

        var allocations = await _parking.Allocations()
            .Include(x => x.ParkingArea)
            .Where(x => x.EventId == eventId && x.IsActive)
            .OrderBy(x => x.ParkingAreaId)
            .ToListAsync(cancellationToken);

        var occupiedIds = _bookings is null
            ? new HashSet<int>()
            : (await _bookings.GetOccupiedParkingSlotIdsAsync(
                    eventId,
                    cancellationToken))
                .ToHashSet();

        var slots = await BuildAllocatedSlotDtosAsync(
            allocations,
            occupiedIds,
            cancellationToken);

        return new ParkingLayoutDto
        {
            EventId = eventId,
            Allocations = allocations.Select(MapAllocation).ToList(),
            Slots = slots
        };
    }

    public async Task<IReadOnlyList<ParkingSlotDto>> GetEventParkingSlotsAsync(
        int eventId,
        int? actorOrganizerId,
        string? actorRole,
        CancellationToken cancellationToken = default)
    {
        var layout = await GetEventLayoutAsync(
            eventId,
            actorOrganizerId,
            actorRole,
            cancellationToken);

        return layout.Slots;
    }

    public async Task<EventParkingAllocationDto> AllocateAreaAsync(
        int eventId,
        CreateEventParkingAllocationDto dto,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default)
    {
        var evt = await GetEditableEventAsync(
            eventId,
            actorOrganizerId,
            actorRole,
            cancellationToken);

        var area = await _parking.GetAreaAsync(
            dto.ParkingAreaId,
            false,
            cancellationToken)
            ?? throw new KeyNotFoundException("Parking area not found.");

        if (!area.IsActive)
            throw new InvalidOperationException("Parking area is inactive.");

        if (area.VenueId != evt.VenueId)
        {
            throw new InvalidOperationException(
                "Parking area must belong to the same venue as the event.");
        }

        if (await _parking.AllocationExistsAsync(
                eventId,
                dto.ParkingAreaId,
                null,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "This parking area is already allocated to the event.");
        }

        var activeSlots = await _parking.Slots()
            .CountAsync(
                x => x.ParkingAreaId == area.Id && x.IsActive,
                cancellationToken);

        if (dto.AllocatedSlotCount > 0 &&
            dto.AllocatedSlotCount > activeSlots)
        {
            throw new ArgumentException(
                "AllocatedSlotCount cannot exceed the number of active slots in the parking area.");
        }

        var allocation = new EventParkingAllocation
        {
            EventId = eventId,
            ParkingAreaId = area.Id,
            AllocatedSlotCount = dto.AllocatedSlotCount,
            ParkingFee = dto.ParkingFee,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _parking.AddAllocationAsync(allocation, cancellationToken);
        await _parking.SaveChangesAsync(cancellationToken);

        allocation.ParkingArea = area;

        return MapAllocation(allocation);
    }

    public async Task DeleteAllocationAsync(
        int allocationId,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default)
    {
        var entity = await _parking.GetAllocationAsync(
            allocationId,
            true,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "Parking allocation not found.");

        await GetEditableEventAsync(
            entity.EventId,
            actorOrganizerId,
            actorRole,
            cancellationToken);

        _parking.RemoveAllocation(entity);
        await _parking.SaveChangesAsync(cancellationToken);
    }

    private async Task<Event> GetEditableEventAsync(
        int eventId,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken)
    {
        var evt = await _events.GetByIdAsync(
            eventId,
            false,
            cancellationToken)
            ?? throw new KeyNotFoundException("Event not found.");

        if (!EventService.IsAdmin(actorRole) &&
            !(EventService.IsOrganizer(actorRole) &&
              actorOrganizerId == evt.OrganizerId))
        {
            throw new UnauthorizedAccessException(
                "You can manage parking only for your own event.");
        }

        if (evt.Status is EventStatus.PendingApproval
            or EventStatus.Approved
            or EventStatus.Published
            or EventStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Parking allocation is locked for this event status.");
        }

        if (_bookings is not null &&
            await _bookings.HasActiveBookingsAsync(
                eventId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "Parking layout cannot be changed after active bookings exist.");
        }

        return evt;
    }

    private async Task EnsureParkingAreaCanChangeAsync(
        int parkingAreaId,
        CancellationToken cancellationToken)
    {
        if (_bookings is null)
            return;

        var eventIds = await _parking.Allocations()
            .Where(x =>
                x.ParkingAreaId == parkingAreaId &&
                x.IsActive)
            .Select(x => x.EventId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var eventId in eventIds)
        {
            if (await _bookings.HasActiveBookingsAsync(
                    eventId,
                    cancellationToken))
            {
                throw new InvalidOperationException(
                    "Parking layout cannot be changed because an allocated event has active bookings.");
            }
        }
    }

    private async Task<List<ParkingSlotDto>> BuildAllocatedSlotDtosAsync(
        IReadOnlyCollection<EventParkingAllocation> allocations,
        HashSet<int> occupiedIds,
        CancellationToken cancellationToken)
    {
        var result = new List<ParkingSlotDto>();

        foreach (var allocation in allocations)
        {
            var query = _parking.Slots()
                .Where(x =>
                    x.ParkingAreaId == allocation.ParkingAreaId &&
                    x.IsActive)
                .OrderBy(x => x.SlotNumber);

            var slots = allocation.AllocatedSlotCount > 0
                ? await query
                    .Take(allocation.AllocatedSlotCount)
                    .ToListAsync(cancellationToken)
                : await query.ToListAsync(cancellationToken);

            result.AddRange(
                slots.Select(x => MapSlot(
                    x,
                    occupiedIds.Contains(x.Id),
                    allocation.ParkingFee)));
        }

        return result;
    }

    private static void EnsureCanViewEvent(
        Event evt,
        int? actorOrganizerId,
        string? actorRole)
    {
        if (evt.Status == EventStatus.Published)
            return;

        if (EventService.IsAdmin(actorRole))
            return;

        if (EventService.IsOrganizer(actorRole) &&
            actorOrganizerId.HasValue &&
            actorOrganizerId.Value == evt.OrganizerId)
        {
            return;
        }

        throw new UnauthorizedAccessException(
            "Parking layout is not public until the event is published.");
    }

    private static ParkingAreaDto MapArea(ParkingArea x) => new()
    {
        Id = x.Id,
        VenueId = x.VenueId,
        Name = x.Name,
        Description = x.Description,
        Capacity = x.Capacity,
        IsActive = x.IsActive
    };

    private static ParkingSlotDto MapSlot(
        ParkingSlot x,
        bool occupied,
        decimal parkingFee) => new()
    {
        Id = x.Id,
        ParkingAreaId = x.ParkingAreaId,
        SlotNumber = x.SlotNumber,
        SlotType = x.SlotType,
        IsActive = x.IsActive,
        Status = !x.IsActive
            ? "Disabled"
            : occupied
                ? "Occupied"
                : "Available",
        ParkingFee = parkingFee
    };

    private static EventParkingAllocationDto MapAllocation(
        EventParkingAllocation x) => new()
    {
        Id = x.Id,
        EventId = x.EventId,
        ParkingAreaId = x.ParkingAreaId,
        ParkingAreaName = x.ParkingArea?.Name,
        AllocatedSlotCount = x.AllocatedSlotCount,
        ParkingFee = x.ParkingFee,
        IsActive = x.IsActive
    };

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
