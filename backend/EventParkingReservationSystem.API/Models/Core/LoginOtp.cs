using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.Models.Core;

public class LoginOtp
{
    public int Id { get; set; }

    public Guid ChallengeId { get; set; }

    public int UserId { get; set; }

    [Required]
    [MaxLength(128)]
    public string OtpHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public int FailedAttempts { get; set; }

    public bool IsUsed { get; set; }

    public DateTime? UsedAt { get; set; }

    public User User { get; set; } = null!;
}