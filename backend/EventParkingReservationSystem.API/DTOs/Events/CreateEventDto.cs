using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Events;

public class CreateEventDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required, RegularExpression("^(SeatBased|NonSeatBased)$", ErrorMessage = "EventType must be SeatBased or NonSeatBased.")]
    public string EventType { get; set; } = "SeatBased";

    // Admin may create an event for an organizer by supplying OrganizerId.
    // Organizer users normally receive it from the organizerId JWT claim.
    public int? OrganizerId { get; set; }

    public int? VenueId { get; set; }

    [Required, RegularExpression("^(OurProperty|ExternalProperty)$", ErrorMessage = "VenueMode must be OurProperty or ExternalProperty.")]
    public string VenueMode { get; set; } = "OurProperty";

    [MaxLength(200)]
    public string? ExternalVenueName { get; set; }

    [MaxLength(500)]
    public string? ExternalVenueAddress { get; set; }

    [Range(1, int.MaxValue)]
    public int EventCategoryId { get; set; }

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TicketPrice { get; set; }

    [MaxLength(1000)]
    public string? PosterUrl { get; set; }
}
