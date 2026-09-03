using EventParkingReservationSystem.API.DTOs.Auth;

namespace EventParkingReservationSystem.API.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(
        LoginRequestDto request);

    Task<LoginResponseDto> RegisterCustomerAsync(
        RegisterCustomerDto request);
}