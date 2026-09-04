using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.Models.Events;

public class ParkingSlot
{
    public int Id { get; set; }
    public int ParkingAreaId { get; set; }
    public ParkingArea? ParkingArea { get; set; }

    [Required, MaxLength(50)]
    public string SlotNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? SlotType { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
