using System.Security.Claims;
using EventParkingReservationSystem.API.DTOs.Organizers;
using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Core;

[ApiController]
[Route("api/organizers")]
[Authorize]
public class OrganizersController : ControllerBase
{
    private readonly IOrganizerService _organizerService;
    private readonly IReportService _reportService;

    public OrganizersController(
        IOrganizerService organizerService,
        IReportService reportService)
    {
        _organizerService = organizerService;
        _reportService = reportService;
    }


    // ============================================
    // ADMIN - VIEW ALL ORGANIZERS
    // ============================================

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var organizers =
            await _organizerService.GetAllAsync();

        return Ok(new
        {
            success = true,
            data = organizers
        });
    }


    // ============================================
    // ADMIN - VIEW SINGLE ORGANIZER
    // ============================================

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var organizer =
            await _organizerService.GetByIdAsync(id);

        if (organizer is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Organizer not found."
            });
        }

        return Ok(new
        {
            success = true,
            data = organizer
        });
    }


    // ============================================
    // ORGANIZER - VIEW OWN PROFILE
    // ============================================

    [HttpGet("me")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Me()
    {
        var userId = GetCurrentUserId();

        var organizer =
            await _organizerService
                .GetByUserIdAsync(userId);

        if (organizer is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Organizer profile not found."
            });
        }

        return Ok(new
        {
            success = true,
            data = organizer
        });
    }


    // ============================================
    // ORGANIZER - VIEW TICKET SALES
    // ============================================

    [HttpGet("me/ticket-sales")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> GetMyTicketSales(
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();

        var result =
            await _reportService
                .GetOrganizerTicketSalesAsync(
                    userId,
                    ct);

        return Ok(new
        {
            success = true,
            data = result
        });
    }


    // ============================================
    // ORGANIZER - VIEW EVENT-WISE REVENUE
    // ============================================

    [HttpGet("me/events/{eventId:int}/revenue")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> GetMyEventRevenue(
        int eventId,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();

        var result =
            await _reportService
                .GetOrganizerEventRevenueAsync(
                    userId,
                    eventId,
                    ct);

        return Ok(new
        {
            success = true,
            data = result
        });
    }


    // ============================================
    // ADMIN - VERIFY / REMOVE VERIFICATION
    // ============================================

    [HttpPatch("{id:int}/verification")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetVerification(
        int id,
        UpdateOrganizerVerificationDto request)
    {
        var organizer =
            await _organizerService
                .SetVerificationAsync(
                    id,
                    request.IsVerified);

        if (organizer is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Organizer not found."
            });
        }

        return Ok(new
        {
            success = true,

            message =
                request.IsVerified
                    ? "Organizer verified successfully."
                    : "Organizer verification removed.",

            data = organizer
        });
    }


    // ============================================
    // GET CURRENT USER ID FROM JWT
    // ============================================

    private int GetCurrentUserId()
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(
                value,
                out var userId))
        {
            throw new UnauthorizedAccessException(
                "Invalid authentication token.");
        }

        return userId;
    }
}