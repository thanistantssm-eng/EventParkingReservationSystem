using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Transactions;

[ApiController, Route("api/reports")]
public sealed class ReportsController(IReportService service) : ControllerBase
{
    [HttpGet("admin-summary")]
    public async Task<ActionResult<AdminReportDto>> AdminSummary(CancellationToken ct) => Ok(await service.GetAdminSummaryAsync(ct));

    [HttpGet("customer/{customerId:int}")]
    public async Task<ActionResult<CustomerReportDto>> CustomerSummary(int customerId, CancellationToken ct) => Ok(await service.GetCustomerSummaryAsync(customerId, ct));
}
