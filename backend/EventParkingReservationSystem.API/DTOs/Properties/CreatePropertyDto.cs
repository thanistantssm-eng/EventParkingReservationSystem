namespace EventParkingReservationSystem.API.DTOs.Properties;

public class CreatePropertyDto
{
    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string? City { get; set; }

    public string? Description { get; set; }
}