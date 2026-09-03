using EventParkingReservationSystem.API.DTOs.Auth;
using EventParkingReservationSystem.API.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [HttpPost("register-customer")]
    public async Task<IActionResult> RegisterCustomer(
        RegisterCustomerDto request)
    {
        var result =
            await _service
                .RegisterCustomerAsync(request);

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequestDto request)
    {
        var result =
            await _service.LoginAsync(request);

        return Ok(result);
    }
}