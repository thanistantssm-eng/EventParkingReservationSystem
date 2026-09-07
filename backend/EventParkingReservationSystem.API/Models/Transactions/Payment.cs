namespace EventParkingReservationSystem.API.Models.Transactions;

public enum PaymentStatus { PendingOtp, Completed, Failed }

public sealed class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string TransactionReference { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.PendingOtp;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
    public Booking Booking { get; set; } = null!;
}
