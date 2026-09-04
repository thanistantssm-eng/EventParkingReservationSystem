using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Username or email is required.")]
    public string Identifier { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}