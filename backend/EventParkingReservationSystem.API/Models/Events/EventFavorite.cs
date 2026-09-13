namespace EventParkingReservationSystem.API.Models.Events;

public class EventFavorite
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int EventId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public Event Event { get; set; } = null!;
}
