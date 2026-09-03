namespace EventParkingReservationSystem.API.DTOs.Properties;

public class PropertyDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public bool IsActive { get; set; }
}