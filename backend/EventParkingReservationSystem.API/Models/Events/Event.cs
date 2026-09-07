using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventParkingReservationSystem.API.Models.Events;

public enum EventType
{
    SeatBased = 1,
    NonSeatBased = 2
}

public enum EventStatus
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    Rejected = 4,
    Published = 5,
    Cancelled = 6
}

public class Event
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    public EventType EventType { get; set; } = EventType.SeatBased;

    // Organizer is owned by Member 1. Keep only the FK here to avoid redefining Core models.
    public int OrganizerId { get; set; }

    // BRD terminology: every event belongs to exactly one Venue.
    // Venue entity/API itself remains Member 1 ownership.
    public int VenueId { get; set; }

    public int EventCategoryId { get; set; }
    public EventCategory? EventCategory { get; set; }

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }

    // BRD-required event-level/base ticket price.
    // TicketType.Price and Seat.PriceOverride remain optional enhanced pricing layers.
    [Column(TypeName = "decimal(18,2)")]
    public decimal TicketPrice { get; set; }

    public EventStatus Status { get; set; } = EventStatus.Draft;

    [MaxLength(1000)]
    public string? PosterUrl { get; set; }

    [MaxLength(300)]
    public string? EventQrCode { get; set; }

    [MaxLength(1000)]
    public string? RejectionReason { get; set; }

    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    public ICollection<EventApproval> Approvals { get; set; } = new List<EventApproval>();
    public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<EventParkingAllocation> ParkingAllocations { get; set; } = new List<EventParkingAllocation>();
}
