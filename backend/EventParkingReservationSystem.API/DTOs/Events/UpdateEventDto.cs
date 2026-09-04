using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Events;

public class UpdateEventDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required, RegularExpression("^(SeatBased|NonSeatBased)$", ErrorMessage = "EventType must be SeatBased or NonSeatBased.")]
    public string EventType { get; set; } = "SeatBased";

    [Range(1, int.MaxValue)]
    public int VenueId { get; set; }

    [Range(1, int.MaxValue)]
    public int EventCategoryId { get; set; }

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TicketPrice { get; set; }

    [MaxLength(1000)]
    public string? PosterUrl { get; set; }
}
