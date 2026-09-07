using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Events;

public class CreateEventCategoryDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class UpdateEventCategoryDto : CreateEventCategoryDto
{
    public bool IsActive { get; set; } = true;
}
