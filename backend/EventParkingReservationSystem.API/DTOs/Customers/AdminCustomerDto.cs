namespace EventParkingReservationSystem.API.DTOs.Customers;

public class AdminCustomerDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}