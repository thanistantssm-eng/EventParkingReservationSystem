using EventParkingReservationSystem.API.DTOs.Users;

namespace EventParkingReservationSystem.API.Interfaces.Services.Core;

public interface IUserService
{
    Task<List<UserResponseDto>> GetAllAsync();

    Task<UserResponseDto?> GetByIdAsync(int id);

    Task<UserResponseDto?> SetStatusAsync(
        int id,
        bool isActive);
}