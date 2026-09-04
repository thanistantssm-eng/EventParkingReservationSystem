using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.Models.Transactions;

public sealed class BookingSeat
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int SeatId { get; set; }
    public Booking Booking { get; set; } = null!;
    public Seat Seat { get; set; } = null!;
}
