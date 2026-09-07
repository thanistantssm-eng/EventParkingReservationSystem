using EventParkingReservationSystem.API.Models.Events;

namespace EventParkingReservationSystem.API.Models.Transactions;

public sealed class BookingParking
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int ParkingSlotId { get; set; }
    public decimal Fee { get; set; }

    public Booking Booking { get; set; } = null!;

    // Canonical parking-slot entity is Member 2's Models.Events.ParkingSlot.
    public ParkingSlot ParkingSlot { get; set; } = null!;
}
