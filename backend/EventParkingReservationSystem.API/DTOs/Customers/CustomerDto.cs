using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.DTOs.Customers;

public class CustomerDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}