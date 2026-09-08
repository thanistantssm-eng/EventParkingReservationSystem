using EventParkingReservationSystem.API.DTOs.Dashboards;

namespace EventParkingReservationSystem.API.Interfaces.Services.Dashboards;

public interface IDashboardService
{
    Task<AdminDashboardDto> GetAdminAsync(
        CancellationToken cancellationToken = default);

    Task<OrganizerDashboardDto> GetOrganizerAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<CustomerDashboardDto> GetCustomerAsync(
        int userId,
        CancellationToken cancellationToken = default);
}