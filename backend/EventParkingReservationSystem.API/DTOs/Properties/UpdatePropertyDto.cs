namespace EventParkingReservationSystem.API.DTOs.Properties;

public class UpdatePropertyDto
{
    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string? City { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}