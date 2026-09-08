using System.Security.Claims;
using EventParkingReservationSystem.API.Interfaces.Services.Dashboards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Dashboards;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardsController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardsController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Admin(
        CancellationToken cancellationToken)
    {
        return Ok(new
        {
            success = true,
            data = await _dashboardService.GetAdminAsync(
                cancellationToken)
        });
    }

    [HttpGet("organizer")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Organizer(
        CancellationToken cancellationToken)
    {
        return Ok(new
        {
            success = true,
            data = await _dashboardService.GetOrganizerAsync(
                CurrentUserId(),
                cancellationToken)
        });
    }

    [HttpGet("customer")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Customer(
        CancellationToken cancellationToken)
    {
        return Ok(new
        {
            success = true,
            data = await _dashboardService.GetCustomerAsync(
                CurrentUserId(),
                cancellationToken)
        });
    }

    private int CurrentUserId()
    {
        var value = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException(
                "Invalid authentication token.");
        }

        return userId;
    }
}