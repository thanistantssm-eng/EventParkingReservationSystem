using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Transactions;

public sealed class CreateBookingDto
{
    [Range(1, int.MaxValue)] public int CustomerId { get; set; }
    [Range(1, int.MaxValue)] public int EventId { get; set; }
    [MinLength(1)] public List<int> SeatIds { get; set; } = [];
    public int? ParkingSlotId { get; set; }
    [Required, MaxLength(50)] public string TicketType { get; set; } = "Standard";
    [Range(1, 20)] public int Quantity { get; set; }
}
