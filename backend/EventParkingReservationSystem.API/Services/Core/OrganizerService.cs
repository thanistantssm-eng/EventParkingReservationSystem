using EventParkingReservationSystem.API.DTOs.Organizers;
using EventParkingReservationSystem.API.Interfaces.Repositories.Core;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.Services.Core;

public class OrganizerService : IOrganizerService
{
    private readonly IOrganizerRepository
        _organizerRepository;

    public OrganizerService(
        IOrganizerRepository organizerRepository)
    {
        _organizerRepository =
            organizerRepository;
    }

    public async Task<List<OrganizerResponseDto>>
        GetAllAsync()
    {
        var organizers =
            await _organizerRepository
                .GetAllAsync();

        return organizers
            .Select(Map)
            .ToList();
    }

    public async Task<OrganizerResponseDto?>
        GetByIdAsync(
            int id)
    {
        var organizer =
            await _organizerRepository
                .GetByIdAsync(id);

        if (organizer is null)
        {
            return null;
        }

        return Map(organizer);
    }

    public async Task<OrganizerResponseDto?>
        GetByUserIdAsync(
            int userId)
    {
        var organizer =
            await _organizerRepository
                .GetByUserIdAsync(
                    userId);

        if (organizer is null)
        {
            return null;
        }

        return Map(organizer);
    }

    public async Task<OrganizerResponseDto?>
        SetVerificationAsync(
            int id,
            bool isVerified)
    {
        var organizer =
            await _organizerRepository
                .GetByIdAsync(id);

        if (organizer is null)
        {
            return null;
        }

        organizer.IsVerified =
            isVerified;

        organizer.UpdatedAt =
            DateTime.UtcNow;

        await _organizerRepository
            .SaveChangesAsync();

        return Map(organizer);
    }

    private static OrganizerResponseDto Map(
        Organizer organizer)
    {
        return new OrganizerResponseDto
        {
            Id =
                organizer.Id,

            UserId =
                organizer.UserId,

            Username =
                organizer.User.Username,

            Email =
                organizer.User.Email,

            OrganizationName =
                organizer.OrganizationName,

            PhoneNumber =
                organizer.PhoneNumber,

            Address =
                organizer.Address,

            IsVerified =
                organizer.IsVerified,

            IsActive =
                organizer.User.IsActive,

            CreatedAt =
                organizer.CreatedAt
        };
    }
}