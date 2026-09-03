namespace EventParkingReservationSystem.API.DTOs.Organizers;

public class OrganizerDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string OrganizationName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string Status { get; set; } = string.Empty;
}