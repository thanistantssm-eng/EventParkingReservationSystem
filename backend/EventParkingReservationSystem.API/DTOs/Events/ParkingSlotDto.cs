using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Events;

public class ParkingSlotDto
{
    public int Id { get; set; }
    public int ParkingAreaId { get; set; }
    public string SlotNumber { get; set; } = string.Empty;
    public string? SlotType { get; set; }
    public bool IsActive { get; set; }

    // BRD customer-facing state: Available / Occupied / Disabled.
    public string Status { get; set; } = string.Empty;

    // Fee assigned through EventParkingAllocation.
    public decimal ParkingFee { get; set; }
}

public class CreateParkingSlotDto
{
    [Required, MaxLength(50)]
    public string SlotNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? SlotType { get; set; }
}

public class UpdateParkingSlotDto : CreateParkingSlotDto
{
    public bool IsActive { get; set; } = true;
}

public class BulkCreateParkingSlotsDto
{
    [Required, MaxLength(3)]
    public string StartingZone { get; set; } = "A";

    [Range(1, 26)]
    public int ZoneCount { get; set; } = 1;

    [Range(1, 100)]
    public int SlotsPerZone { get; set; } = 10;

    [Range(1, 9999)]
    public int StartingNumber { get; set; } = 1;

    [MaxLength(50)]
    public string? SlotType { get; set; }
}
