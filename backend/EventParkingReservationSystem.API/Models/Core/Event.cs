namespace EventParkingReservationSystem.API.Models.Core;

public sealed class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartsAtUtc { get; set; }
    public decimal TicketPrice { get; set; }
    public decimal ParkingFee { get; set; }
    public ICollection<Seat> Seats { get; set; } = [];
    public ICollection<ParkingSlot> ParkingSlots { get; set; } = [];
}
