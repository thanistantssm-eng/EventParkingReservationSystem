using System.Security.Claims;
using EventParkingReservationSystem.API.DTOs.Users;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Core;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController
    : ControllerBase
{
    private readonly IUserService
        _userService;

    public UsersController(
        IUserService userService)
    {
        _userService =
            userService;
    }


    // Current logged user

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId =
            GetCurrentUserId();

        var user =
            await _userService
                .GetByIdAsync(userId);

        if (user is null)
        {
            return NotFound(new
            {
                success = false,
                message = "User not found."
            });
        }


        return Ok(new
        {
            success = true,
            data = user
        });
    }


    // Admin gets all users

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult>
        GetAll()
    {
        var users =
            await _userService
                .GetAllAsync();

        return Ok(new
        {
            success = true,
            data = users
        });
    }


    // Admin gets one user

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult>
        GetById(
            int id)
    {
        var user =
            await _userService
                .GetByIdAsync(id);

        if (user is null)
        {
            return NotFound(new
            {
                success = false,
                message = "User not found."
            });
        }


        return Ok(new
        {
            success = true,
            data = user
        });
    }


    // Admin activates/deactivates user

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult>
        SetStatus(
            int id,
            UpdateUserStatusRequestDto request)
    {
        var user =
            await _userService
                .SetStatusAsync(
                    id,
                    request.IsActive);

        if (user is null)
        {
            return NotFound(new
            {
                success = false,
                message = "User not found."
            });
        }


        return Ok(new
        {
            success = true,

            message =
                request.IsActive
                    ? "User activated successfully."
                    : "User deactivated successfully.",

            data = user
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