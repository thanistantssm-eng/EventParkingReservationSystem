using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Auth;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "Username is required.")]
    [MinLength(
        3,
        ErrorMessage = "Username must contain at least 3 characters."
    )]
    [MaxLength(
        50,
        ErrorMessage = "Username cannot exceed 50 characters."
    )]
    public string Username { get; set; } = string.Empty;


    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;


    [Required(ErrorMessage = "Password is required.")]
    [MinLength(
        8,
        ErrorMessage = "Password must contain at least 8 characters."
    )]
    [MaxLength(
        64,
        ErrorMessage = "Password cannot exceed 64 characters."
    )]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9])\S{8,64}$",
        ErrorMessage =
            "Password must contain at least one uppercase letter, one lowercase letter, one number, one special character, and no spaces."
    )]
    public string Password { get; set; } = string.Empty;


    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = "Customer";


    public string? OrganizationName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }
}