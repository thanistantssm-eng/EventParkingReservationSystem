using EventParkingReservationSystem.API.DTOs.Auth;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Core;

[ApiController]
[Route("api/auth")]
public class AuthController
    : ControllerBase
{
    private readonly IAuthService
        _authService;

    public AuthController(
        IAuthService authService)
    {
        _authService =
            authService;
    }


    // ====================================
    // ONE-TIME ADMIN SETUP
    // ====================================

    [AllowAnonymous]
    [HttpGet("admin-setup-status")]
    public async Task<IActionResult> AdminSetupStatus()
    {
        return Ok(new
        {
            success = true,
            data = new
            {
                setupRequired = await _authService
                    .IsAdminSetupRequiredAsync()
            }
        });
    }

    [AllowAnonymous]
    [HttpPost("admin-setup")]
    public async Task<IActionResult> SetupAdmin(
        AdminSetupRequestDto request)
    {
        try
        {
            var result = await _authService
                .SetupAdminAsync(request);

            return Ok(new
            {
                success = true,
                message = "Administrator account created. You can now sign in.",
                data = result
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }


    // ====================================
    // REGISTER
    // ====================================

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult>
        Register(
            RegisterRequestDto request)
    {
        try
        {
            var result =
                await _authService
                    .RegisterAsync(
                        request);

            return Ok(new
            {
                success = true,

                message =
                    "Registration successful.",

                data = result
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }


    // ====================================
    // LOGIN
    // PASSWORD → SEND OTP
    // ====================================

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult>
        Login(
            LoginRequestDto request)
    {
        try
        {
            var result =
                await _authService
                    .LoginAsync(
                        request);

            return Ok(new
            {
                success = true,

                message =
                    "Password verified. OTP has been sent to your email.",

                data = result
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes
                    .Status500InternalServerError,
                new
                {
                    success = false,

                    message =
                        "Unable to send login OTP."
                });
        }
    }


    // ====================================
    // VERIFY OTP
    // ====================================

    [AllowAnonymous]
    [HttpPost("verify-otp")]
    public async Task<IActionResult>
        VerifyOtp(
            VerifyLoginOtpRequestDto request)
    {
        try
        {
            var result =
                await _authService
                    .VerifyLoginOtpAsync(
                        request);

            return Ok(new
            {
                success = true,

                message =
                    "OTP verified. Login successful.",

                data = result
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                success = false,
                message = ex.Message
            });
        }
    }


    // ====================================
    // RESEND OTP
    // ====================================

    [AllowAnonymous]
    [HttpPost("resend-otp")]
    public async Task<IActionResult>
        ResendOtp(
            ResendLoginOtpRequestDto request)
    {
        try
        {
            var result =
                await _authService
                    .ResendLoginOtpAsync(
                        request);

            return Ok(new
            {
                success = true,

                message =
                    "A new OTP has been sent to your email.",

                data = result
            });
        }
        catch (
            UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (
            InvalidOperationException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }


    // ====================================
    // PASSWORD RESET
    // ====================================

    [AllowAnonymous]
    [HttpPost("password-reset/request")]
    public async Task<IActionResult> RequestPasswordReset(
        PasswordResetRequestDto request)
    {
        try
        {
            var result = await _authService
                .RequestPasswordResetAsync(request);

            return Ok(new
            {
                success = true,
                message = "If the account exists, a reset code has been sent.",
                data = result
            });
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    success = false,
                    message = "Unable to send the password reset code."
                });
        }
    }

    [AllowAnonymous]
    [HttpPost("password-reset/confirm")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequestDto request)
    {
        try
        {
            await _authService.ResetPasswordAsync(request);

            return Ok(new
            {
                success = true,
                message = "Password reset successful. You can now sign in."
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                success = false,
                message = ex.Message
            });
        }
    }
}
