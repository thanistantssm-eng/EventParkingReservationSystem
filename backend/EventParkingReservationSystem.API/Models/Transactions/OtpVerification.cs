namespace EventParkingReservationSystem.API.Models.Transactions;

public sealed class OtpVerification
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public int FailedAttempts { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public Payment Payment { get; set; } = null!;
}
