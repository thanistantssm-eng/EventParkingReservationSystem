namespace EventParkingReservationSystem.API.DTOs.Auth;

public class LoginPendingResponseDto
{
    public bool RequiresOtp { get; set; }

    public Guid ChallengeId { get; set; }

    public string MaskedEmail { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}