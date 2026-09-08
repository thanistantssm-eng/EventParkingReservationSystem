namespace EventParkingReservationSystem.API.DTOs.Customers;

public class AdminCustomerDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } =
        string.Empty;

    public string Username { get; set; } =
        string.Empty;

    public string Email { get; set; } =
        string.Empty;

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public int TotalBookings { get; set; }

    public int ConfirmedBookings { get; set; }

    public int PendingBookings { get; set; }

    public int CancelledBookings { get; set; }

    public decimal TotalSpent { get; set; }

    public DateTime? LastBookingAtUtc { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}