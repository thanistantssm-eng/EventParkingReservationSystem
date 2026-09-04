namespace EventParkingReservationSystem.API.DTOs.Events;

public class EventDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public int OrganizerId { get; set; }
    public int VenueId { get; set; }
    public int EventCategoryId { get; set; }
    public string? EventCategoryName { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public decimal TicketPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public string? EventQrCode { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class EventQueryDto
{
    public string? Search { get; set; }
    public DateTime? Date { get; set; }

    // Exact Angular/BRD query contract:
    // /api/events?search=&venue=&category=&date=
    public int? Venue { get; set; }
    public int? Category { get; set; }

    // Explicit-id aliases are also accepted for API consumers.
    public int? VenueId { get; set; }
    public int? EventCategoryId { get; set; }

    // Enhanced filters retained from the team architecture.
    public string? EventType { get; set; }
    public string? Status { get; set; }
}
