namespace EventParkingReservationSystem.API.Models.Core;

public sealed class ParkingSlot
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string SlotNumber { get; set; } = string.Empty;
    public Event Event { get; set; } = null!;
}
