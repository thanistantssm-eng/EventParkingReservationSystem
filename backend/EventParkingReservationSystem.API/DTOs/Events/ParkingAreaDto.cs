using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Events;

public class ParkingAreaDto
{
    public int Id { get; set; }
    public int VenueId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
}

public class CreateParkingAreaDto
{
    [Range(1, int.MaxValue)]
    public int VenueId { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }
}

public class UpdateParkingAreaDto : CreateParkingAreaDto
{
    public bool IsActive { get; set; } = true;
}

public class EventParkingAllocationDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int ParkingAreaId { get; set; }
    public string? ParkingAreaName { get; set; }
    public int AllocatedSlotCount { get; set; }
    public decimal ParkingFee { get; set; }
    public bool IsActive { get; set; }
}

public class CreateEventParkingAllocationDto
{
    [Range(1, int.MaxValue)]
    public int ParkingAreaId { get; set; }

    [Range(0, int.MaxValue)]
    public int AllocatedSlotCount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ParkingFee { get; set; }
}

public class ParkingLayoutDto
{
    public int EventId { get; set; }
    public List<EventParkingAllocationDto> Allocations { get; set; } = new();
    public List<ParkingSlotDto> Slots { get; set; } = new();
}
