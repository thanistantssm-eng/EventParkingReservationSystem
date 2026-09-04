using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.Models.Transactions;

public sealed class BookingParking
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int ParkingSlotId { get; set; }
    public decimal Fee { get; set; }
    public Booking Booking { get; set; } = null!;
    public ParkingSlot ParkingSlot { get; set; } = null!;
}
