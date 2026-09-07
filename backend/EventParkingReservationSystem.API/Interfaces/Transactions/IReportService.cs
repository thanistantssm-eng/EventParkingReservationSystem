using EventParkingReservationSystem.API.DTOs.Transactions;

namespace EventParkingReservationSystem.API.Interfaces.Transactions;

public interface IReportService
{
    Task<AdminReportDto> GetAdminSummaryAsync(CancellationToken ct);
    Task<CustomerReportDto> GetCustomerSummaryAsync(int customerId, CancellationToken ct);
}
