using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Events;

public class CreateSeatDto
{
    public int? TicketTypeId { get; set; }

    [Required, MaxLength(50)]
    public string SeatNumber { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? RowLabel { get; set; }

    public int? ColumnNumber { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? PriceOverride { get; set; }
}
