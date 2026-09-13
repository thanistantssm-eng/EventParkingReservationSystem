using EventParkingReservationSystem.API.DTOs.Transactions;

namespace EventParkingReservationSystem.API.Interfaces.Transactions;

public interface IReportService
{
    Task<AdminReportDto> GetAdminSummaryAsync(CancellationToken ct);
    Task<CustomerReportDto> GetCustomerSummaryAsync(int customerId, CancellationToken ct);


    Task<OrganizerTicketSalesDto>
        GetOrganizerTicketSalesAsync(
            int userId,
            CancellationToken ct);

    Task<OrganizerEventRevenueDto>
        GetOrganizerEventRevenueAsync(
            int userId,
            int eventId,
            CancellationToken ct);

    Task<EventReportDto> GetOrganizerEventReportAsync(
        int userId,
        int eventId,
        CancellationToken ct);

    Task<EventReportDto> GetAdminOrganizerEventReportAsync(
        int organizerId,
        int eventId,
        CancellationToken ct);

    Task<EventReportDto> SendAdminOrganizerEventReportAsync(
        int organizerId,
        int eventId,
        CancellationToken ct);
}



