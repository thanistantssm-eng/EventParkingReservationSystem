namespace EventParkingReservationSystem.API.DTOs.Dashboards;

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalOrganizers { get; set; }
    public int PendingOrganizerVerifications { get; set; }
    public int TotalProperties { get; set; }
    public int TotalVenues { get; set; }
    public int TotalEvents { get; set; }
    public int PublishedEvents { get; set; }
    public int PendingApprovalEvents { get; set; }
    public int TotalBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CompletedPayments { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ReservedSeats { get; set; }
    public int ReservedParkingSlots { get; set; }
}

public class OrganizerDashboardDto
{
    public int OrganizerId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public int TotalEvents { get; set; }
    public int DraftEvents { get; set; }
    public int PendingApprovalEvents { get; set; }
    public int PublishedEvents { get; set; }
    public int UpcomingEvents { get; set; }
    public int TotalBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class CustomerDashboardDto
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int UpcomingBookings { get; set; }
    public decimal TotalSpent { get; set; }
}