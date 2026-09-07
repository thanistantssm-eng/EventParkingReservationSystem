using EventParkingReservationSystem.API.Models.Events;

namespace EventParkingReservationSystem.API.Models.Transactions;

public sealed class BookingSeat
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int SeatId { get; set; }

    public Booking Booking { get; set; } = null!;

    // Canonical seat entity is Member 2's Models.Events.Seat.
    public Seat Seat { get; set; } = null!;
}
