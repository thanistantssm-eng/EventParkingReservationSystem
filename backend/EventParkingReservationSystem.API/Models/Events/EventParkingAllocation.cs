using System.ComponentModel.DataAnnotations.Schema;

namespace EventParkingReservationSystem.API.Models.Events;

public class EventParkingAllocation
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }

    public int ParkingAreaId { get; set; }
    public ParkingArea? ParkingArea { get; set; }

    // 0 means all active slots in the parking area are available to this event.
    public int AllocatedSlotCount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ParkingFee { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
