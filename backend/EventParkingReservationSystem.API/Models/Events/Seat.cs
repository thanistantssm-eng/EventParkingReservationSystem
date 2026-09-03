using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventParkingReservationSystem.API.Models.Events;

public enum SeatSetupStatus
{
    Available = 1,
    Disabled = 2
}

public class Seat
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }

    public int? TicketTypeId { get; set; }
    public TicketType? TicketType { get; set; }

    [Required, MaxLength(50)]
    public string SeatNumber { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? RowLabel { get; set; }

    public int? ColumnNumber { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PriceOverride { get; set; }

    public SeatSetupStatus SetupStatus { get; set; } = SeatSetupStatus.Available;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
