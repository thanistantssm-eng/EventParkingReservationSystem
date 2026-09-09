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
}
