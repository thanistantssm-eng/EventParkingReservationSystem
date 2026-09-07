using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Notifications;

public class CreateNotificationDto
{
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Type { get; set; } = "General";
}