using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EventParkingReservationSystem.API.DTOs.Transactions;

public sealed class CreateBookingDto
{
    // Server-controlled from the authenticated JWT customerId claim.
    [JsonIgnore]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue)]
    public int EventId { get; set; }

    // Seat-based events require seats.
    // Non-seat-based events intentionally allow an empty list.
    public List<int> SeatIds { get; set; } = [];

    public int? ParkingSlotId { get; set; }

    [Required, MaxLength(100)]
    public string TicketType { get; set; } = "Standard";

    [Range(1, 20)]
    public int Quantity { get; set; }
}
