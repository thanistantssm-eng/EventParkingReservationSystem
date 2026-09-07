using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Auth;

public class VerifyLoginOtpRequestDto
{
    [Required]
    public Guid ChallengeId { get; set; }

    [Required]
    [RegularExpression(
        @"^\d{6}$",
        ErrorMessage = "OTP must contain exactly 6 digits."
    )]
    public string Otp { get; set; } = string.Empty;
}