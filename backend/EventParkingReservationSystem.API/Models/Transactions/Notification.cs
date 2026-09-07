namespace EventParkingReservationSystem.API.Models.Transactions;

public sealed class Notification
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int? BookingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
