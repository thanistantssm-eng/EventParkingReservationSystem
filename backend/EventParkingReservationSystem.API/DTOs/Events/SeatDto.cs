using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Events;

public class SeatDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int? TicketTypeId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public string? RowLabel { get; set; }
    public int? ColumnNumber { get; set; }
    public decimal? PriceOverride { get; set; }

    // Effective price used by the Angular running total:
    // Seat override -> TicketType price -> Event base TicketPrice.
    public decimal Price { get; set; }

    // SetupStatus is Member 2 configuration state.
    public string SetupStatus { get; set; } = string.Empty;

    // BRD customer-facing state: Available / Booked / Disabled.
    // "Selected" stays client-side in Angular.
    public string Status { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}

public class UpdateSeatDto
{
    public int? TicketTypeId { get; set; }

    [Required, MaxLength(50)]
    public string SeatNumber { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? RowLabel { get; set; }

    public int? ColumnNumber { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? PriceOverride { get; set; }

    [Required, RegularExpression("^(Available|Disabled)$")]
    public string SetupStatus { get; set; } = "Available";

    public bool IsActive { get; set; } = true;
}
