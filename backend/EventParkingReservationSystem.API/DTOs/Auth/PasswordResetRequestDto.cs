using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Auth;

public sealed class PasswordResetRequestDto
{
    [Required]
    [MaxLength(150)]
    public string Identifier { get; set; } = string.Empty;
}
