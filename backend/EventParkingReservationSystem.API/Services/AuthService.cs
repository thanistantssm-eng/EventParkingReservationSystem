using EventParkingReservationSystem.API.DTOs.Auth;
using EventParkingReservationSystem.API.Exceptions;
using EventParkingReservationSystem.API.Helpers;
using EventParkingReservationSystem.API.Interfaces.Repositories;
using EventParkingReservationSystem.API.Interfaces.Services;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.AspNetCore.Identity;

namespace EventParkingReservationSystem.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponseDto>
        RegisterCustomerAsync(
            RegisterCustomerDto request)
    {
        if (await _userRepository
            .UsernameExistsAsync(request.Username))
        {
            throw new AppException(
                "Username already exists.",
                StatusCodes.Status409Conflict);
        }

        if (await _userRepository
            .EmailExistsAsync(request.Email))
        {
            throw new AppException(
                "Email already exists.",
                StatusCodes.Status409Conflict);
        }

        var user = new User
        {
            Username = request.Username.Trim(),

            Email = request.Email
                .Trim()
                .ToLower(),

            Role = UserRole.Customer,

            IsActive = true
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                request.Password);

        await _userRepository.AddAsync(user);

        return BuildResponse(user);
    }

    public async Task<LoginResponseDto>
        LoginAsync(LoginRequestDto request)
    {
        var user =
            await _userRepository
                .GetByIdentifierAsync(
                    request.Identifier);

        if (user is null)
        {
            throw new AppException(
                "Invalid username/email or password.",
                StatusCodes.Status401Unauthorized);
        }

        if (!user.IsActive)
        {
            throw new AppException(
                "Account is inactive.",
                StatusCodes.Status403Forbidden);
        }

        var result =
            _passwordHasher
                .VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password);

        if (result ==
            PasswordVerificationResult.Failed)
        {
            throw new AppException(
                "Invalid username/email or password.",
                StatusCodes.Status401Unauthorized);
        }

        return BuildResponse(user);
    }

    private LoginResponseDto BuildResponse(User user)
    {
        TokenResult token =
            _jwtTokenService.CreateToken(user);

        return new LoginResponseDto
        {
            Token = token.Token,
            ExpiresAtUtc = token.ExpiresAtUtc,
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}