using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Auth;

public class RegisterRequestDto
{
    [Required]
    [MinLength(3)]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(64)]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9])\S{8,64}$",
        ErrorMessage =
            "Password must contain uppercase, lowercase, number, special character and no spaces."
    )]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Customer";

    public string? OrganizationName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }
}