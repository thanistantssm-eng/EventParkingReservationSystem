using EventParkingReservationSystem.API.DTOs.Users;
using EventParkingReservationSystem.API.Interfaces.Repositories.Core;
using EventParkingReservationSystem.API.Interfaces.Services.Core;

namespace EventParkingReservationSystem.API.Services.Core;

public class UserService
    : IUserService
{
    private readonly IUserRepository
        _userRepository;

    public UserService(
        IUserRepository userRepository)
    {
        _userRepository =
            userRepository;
    }


    public async Task<List<UserResponseDto>>
        GetAllAsync()
    {
        var users =
            await _userRepository
                .GetAllAsync();

        return users
            .Select(x =>
                new UserResponseDto
                {
                    Id = x.Id,
                    Username = x.Username,
                    Email = x.Email,
                    Role =
                        x.Role.ToString(),
                    IsActive =
                        x.IsActive,
                    CreatedAt =
                        x.CreatedAt,
                    UpdatedAt =
                        x.UpdatedAt
                })
            .ToList();
    }


    public async Task<UserResponseDto?>
        GetByIdAsync(
            int id)
    {
        var user =
            await _userRepository
                .GetByIdAsync(id);

        if (user is null)
        {
            return null;
        }

        return new UserResponseDto
        {
            Id =
                user.Id,

            Username =
                user.Username,

            Email =
                user.Email,

            Role =
                user.Role.ToString(),

            IsActive =
                user.IsActive,

            CreatedAt =
                user.CreatedAt,

            UpdatedAt =
                user.UpdatedAt
        };
    }


    public async Task<UserResponseDto?>
        SetStatusAsync(
            int id,
            bool isActive)
    {
        var user =
            await _userRepository
                .GetByIdAsync(id);

        if (user is null)
        {
            return null;
        }


        user.IsActive =
            isActive;

        user.UpdatedAt =
            DateTime.UtcNow;


        await _userRepository
            .SaveChangesAsync();


        return new UserResponseDto
        {
            Id =
                user.Id,

            Username =
                user.Username,

            Email =
                user.Email,

            Role =
                user.Role.ToString(),

            IsActive =
                user.IsActive,

            CreatedAt =
                user.CreatedAt,

            UpdatedAt =
                user.UpdatedAt
        };
    }
}