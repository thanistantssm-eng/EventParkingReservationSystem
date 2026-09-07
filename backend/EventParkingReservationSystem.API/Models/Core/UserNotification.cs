using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.Models.Core;

public class UserNotification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = "General";

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }

    public User User { get; set; } = null!;
}