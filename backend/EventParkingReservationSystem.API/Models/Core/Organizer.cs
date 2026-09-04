using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.Models.Core;

public class Organizer
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [MaxLength(150)]
    public string OrganizationName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    public bool IsVerified { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}