using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.Models.Events;

public class ParkingArea
{
    public int Id { get; set; }

    // BRD terminology: parking belongs to a venue.
    // Venue entity itself is owned by Member 1.
    public int VenueId { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ParkingSlot> Slots { get; set; } = new List<ParkingSlot>();
    public ICollection<EventParkingAllocation> EventAllocations { get; set; } = new List<EventParkingAllocation>();
}
