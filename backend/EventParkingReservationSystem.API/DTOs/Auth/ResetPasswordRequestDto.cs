using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Auth;

public sealed class ResetPasswordRequestDto
{
    [Required]
    public Guid ChallengeId { get; set; }

    [Required]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must contain exactly 6 digits.")]
    public string Otp { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(64)]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9])\S{8,64}$",
        ErrorMessage =
            "Password must contain uppercase, lowercase, number, special character and no spaces.")]
    public string NewPassword { get; set; } = string.Empty;
}
