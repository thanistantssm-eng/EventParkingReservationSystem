using EventParkingReservationSystem.API.DTOs.Organizers;
using EventParkingReservationSystem.API.Exceptions;
using EventParkingReservationSystem.API.Interfaces.Repositories;
using EventParkingReservationSystem.API.Interfaces.Services;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.AspNetCore.Identity;

namespace EventParkingReservationSystem.API.Services;

public class OrganizerService : IOrganizerService
{
    private readonly IOrganizerRepository _organizerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public OrganizerService(
        IOrganizerRepository organizerRepository,
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher)
    {
        _organizerRepository = organizerRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<OrganizerDto>> GetAllAsync()
    {
        var organizers =
            await _organizerRepository.GetAllAsync();

        return organizers
            .Select(Map)
            .ToList();
    }

    public async Task<OrganizerDto> GetByIdAsync(int id)
    {
        var organizer =
            await _organizerRepository.GetByIdAsync(id);

        if (organizer is null)
        {
            throw new AppException(
                "Organizer not found.",
                StatusCodes.Status404NotFound);
        }

        return Map(organizer);
    }

    public async Task<OrganizerDto> CreateAsync(
        CreateOrganizerDto request)
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

            Role = UserRole.Organizer,

            IsActive = true
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                request.Password);

        var organizer = new Organizer
        {
            User = user,

            OrganizationName =
                request.OrganizationName.Trim(),

            Phone = request.Phone.Trim(),

            Address = request.Address?.Trim(),

            Status = OrganizerStatus.Active
        };

        await _organizerRepository
            .AddAsync(organizer);

        return Map(organizer);
    }

    public async Task<OrganizerDto> UpdateAsync(
        int id,
        UpdateOrganizerDto request)
    {
        var organizer =
            await _organizerRepository.GetByIdAsync(id);

        if (organizer is null)
        {
            throw new AppException(
                "Organizer not found.",
                StatusCodes.Status404NotFound);
        }

        organizer.OrganizationName =
            request.OrganizationName.Trim();

        organizer.Phone =
            request.Phone.Trim();

        organizer.Address =
            request.Address?.Trim();

        organizer.Status =
            request.Status;

        organizer.User.IsActive =
            request.Status ==
            OrganizerStatus.Active;

        await _organizerRepository
            .SaveChangesAsync();

        return Map(organizer);
    }

    public async Task DeactivateAsync(int id)
    {
        var organizer =
            await _organizerRepository.GetByIdAsync(id);

        if (organizer is null)
        {
            throw new AppException(
                "Organizer not found.",
                StatusCodes.Status404NotFound);
        }

        organizer.Status =
            OrganizerStatus.Suspended;

        organizer.User.IsActive = false;

        await _organizerRepository
            .SaveChangesAsync();
    }

    private static OrganizerDto Map(
        Organizer organizer)
    {
        return new OrganizerDto
        {
            Id = organizer.Id,
            UserId = organizer.UserId,
            Username = organizer.User.Username,
            Email = organizer.User.Email,
            OrganizationName =
                organizer.OrganizationName,
            Phone = organizer.Phone,
            Address = organizer.Address,
            Status = organizer.Status.ToString()
        };
    }
}