namespace EventParkingReservationSystem.API.Models.Core;

public class Organizer
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string OrganizationName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Address { get; set; }

    public OrganizerStatus Status { get; set; } = OrganizerStatus.Active;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}