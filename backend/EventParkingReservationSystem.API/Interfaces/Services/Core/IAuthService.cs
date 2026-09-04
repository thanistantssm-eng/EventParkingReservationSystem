using EventParkingReservationSystem.API.DTOs.Auth;

namespace EventParkingReservationSystem.API.Interfaces.Services.Core;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(
        RegisterRequestDto request);

    Task<LoginPendingResponseDto> LoginAsync(
        LoginRequestDto request);

    Task<AuthResponseDto> VerifyLoginOtpAsync(
        VerifyLoginOtpRequestDto request);

    Task<LoginPendingResponseDto> ResendLoginOtpAsync(
        ResendLoginOtpRequestDto request);
}