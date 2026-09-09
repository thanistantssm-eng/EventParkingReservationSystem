using EventParkingReservationSystem.API.DTOs.Auth;

namespace EventParkingReservationSystem.API.Interfaces.Services.Core;

public interface IAuthService
{
    Task<bool> IsAdminSetupRequiredAsync();

    Task<RegisterResponseDto> SetupAdminAsync(
        AdminSetupRequestDto request);

    Task<RegisterResponseDto> RegisterAsync(
        RegisterRequestDto request);

    Task<LoginPendingResponseDto> LoginAsync(
        LoginRequestDto request);

    Task<AuthResponseDto> VerifyLoginOtpAsync(
        VerifyLoginOtpRequestDto request);

    Task<LoginPendingResponseDto> ResendLoginOtpAsync(
        ResendLoginOtpRequestDto request);

    Task<LoginPendingResponseDto> RequestPasswordResetAsync(
        PasswordResetRequestDto request);

    Task ResetPasswordAsync(
        ResetPasswordRequestDto request);
}
