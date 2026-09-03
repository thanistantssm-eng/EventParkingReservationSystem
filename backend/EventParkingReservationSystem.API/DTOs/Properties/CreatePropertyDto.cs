using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Properties;

public class CreatePropertyDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Location { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }
}