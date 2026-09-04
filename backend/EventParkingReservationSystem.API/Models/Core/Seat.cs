namespace EventParkingReservationSystem.API.Models.Core;

public sealed class Seat
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public Event Event { get; set; } = null!;
}
