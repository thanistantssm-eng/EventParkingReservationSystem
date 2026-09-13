using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Extensions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Transactions;

[ApiController]
[Route("api/reports")]
[Authorize]
public sealed class ReportsController(IReportService service) : ControllerBase
{
    [HttpGet("admin-summary")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AdminReportDto>> AdminSummary(
        CancellationToken ct) =>
        Ok(await service.GetAdminSummaryAsync(ct));

    [HttpGet("customer/me")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<CustomerReportDto>> MySummary(
        CancellationToken ct) =>
        Ok(await service.GetCustomerSummaryAsync(
            User.RequireCustomerId(),
            ct));

    [HttpGet("customer/{customerId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CustomerReportDto>> CustomerSummary(
        int customerId,
        CancellationToken ct) =>
        Ok(await service.GetCustomerSummaryAsync(customerId, ct));

    [HttpGet("organizer/events/{eventId:int}")]
    [Authorize(Roles = "Organizer")]
    public async Task<ActionResult<EventReportDto>> OrganizerEventReport(
        int eventId,
        CancellationToken ct) =>
        Ok(await service.GetOrganizerEventReportAsync(
            User.RequireUserId(),
            eventId,
            ct));

    [HttpGet("admin/organizers/{organizerId:int}/events/{eventId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EventReportDto>> AdminOrganizerEventReport(
        int organizerId,
        int eventId,
        CancellationToken ct) =>
        Ok(await service.GetAdminOrganizerEventReportAsync(
            organizerId,
            eventId,
            ct));

    [HttpPost("admin/organizers/{organizerId:int}/events/{eventId:int}/send")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EventReportDto>> SendAdminOrganizerEventReport(
        int organizerId,
        int eventId,
        CancellationToken ct) =>
        Ok(await service.SendAdminOrganizerEventReportAsync(
            organizerId,
            eventId,
            ct));
}
