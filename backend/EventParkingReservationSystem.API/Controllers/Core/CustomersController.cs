using System.Security.Claims;
using EventParkingReservationSystem.API.DTOs.Customers;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Core;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = "Customer")]
public class CustomersController
    : ControllerBase
{
    private readonly ICustomerService
        _customerService;


    public CustomersController(
        ICustomerService customerService)
    {
        _customerService =
            customerService;
    }


    // ============================================
    // CUSTOMER GET OWN PROFILE
    // ============================================

    [HttpGet("me")]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me()
    {
        var userId =
            GetCurrentUserId();


        var customer =
            await _customerService
                .GetMyProfileAsync(
                    userId);


        if (customer is null)
        {
            return NotFound(new
            {
                success = false,

                message =
                    "Customer profile not found."
            });
        }


        return Ok(new
        {
            success = true,

            data = customer
        });
    }


    // ============================================
    // CUSTOMER UPDATE OWN PROFILE
    // ============================================

    [HttpPut("me")]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult>
        UpdateMe(
            [FromBody]
            UpdateCustomerProfileDto request)
    {
        var userId =
            GetCurrentUserId();


        var customer =
            await _customerService
                .UpdateMyProfileAsync(
                    userId,
                    request);


        if (customer is null)
        {
            return NotFound(new
            {
                success = false,

                message =
                    "Customer profile not found."
            });
        }


        return Ok(new
        {
            success = true,

            message =
                "Profile updated successfully.",

            data = customer
        });
    }


    // ============================================
    // GET USER ID FROM JWT
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