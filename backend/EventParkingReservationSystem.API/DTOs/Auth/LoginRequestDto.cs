using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Auth;

public class LoginRequestDto
{
    [Required]
    public string Identifier { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}