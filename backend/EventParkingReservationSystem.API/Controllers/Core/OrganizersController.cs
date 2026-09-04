using System.Security.Claims;
using EventParkingReservationSystem.API.DTOs.Organizers;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Core;

[ApiController]
[Route("api/organizers")]
[Authorize]
public class OrganizersController
    : ControllerBase
{
    private readonly IOrganizerService
        _organizerService;

    public OrganizersController(
        IOrganizerService organizerService)
    {
        _organizerService =
            organizerService;
    }


    // Admin views all organizers

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult>
        GetAll()
    {
        var organizers =
            await _organizerService
                .GetAllAsync();

        return Ok(new
        {
            success = true,
            data = organizers
        });
    }


    // Admin gets organizer

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult>
        GetById(
            int id)
    {
        var organizer =
            await _organizerService
                .GetByIdAsync(id);

        if (organizer is null)
        {
            return NotFound(new
            {
                success = false,
                message =
                    "Organizer not found."
            });
        }


        return Ok(new
        {
            success = true,
            data = organizer
        });
    }


    // Organizer gets own profile

    [HttpGet("me")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Me()
    {
        var userId =
            GetCurrentUserId();

        var organizer =
            await _organizerService
                .GetByUserIdAsync(
                    userId);

        if (organizer is null)
        {
            return NotFound(new
            {
                success = false,
                message =
                    "Organizer profile not found."
            });
        }


        return Ok(new
        {
            success = true,
            data = organizer
        });
    }


    // Admin verifies/rejects organizer

    [HttpPatch("{id:int}/verification")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult>
        SetVerification(
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
                message =
                    "Organizer not found."
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