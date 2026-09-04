namespace EventParkingReservationSystem.API.Models.Transactions;

public sealed class QrCode
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public Booking Booking { get; set; } = null!;
}
