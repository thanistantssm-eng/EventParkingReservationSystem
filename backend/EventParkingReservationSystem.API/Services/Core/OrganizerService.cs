using EventParkingReservationSystem.API.DTOs.Organizers;
using EventParkingReservationSystem.API.Interfaces.Repositories.Core;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.Services.Core;

public class OrganizerService : IOrganizerService
{
    private readonly IOrganizerRepository
        _repository;

    private readonly INotificationService
        _notificationService;


    public OrganizerService(
        IOrganizerRepository repository,
        INotificationService notificationService)
    {
        _repository =
            repository;

        _notificationService =
            notificationService;
    }


    // ============================================
    // GET ALL
    // ============================================

    public async Task<List<OrganizerResponseDto>>
        GetAllAsync()
    {
        var organizers =
            await _repository
                .GetAllAsync();


        return organizers
            .Select(Map)
            .ToList();
    }


    // ============================================
    // GET BY ID
    // ============================================

    public async Task<OrganizerResponseDto?>
        GetByIdAsync(
            int id)
    {
        var organizer =
            await _repository
                .GetByIdAsync(id);


        return organizer == null
            ? null
            : Map(organizer);
    }


    // ============================================
    // GET BY USER ID
    // ============================================

    public async Task<OrganizerResponseDto?>
        GetByUserIdAsync(
            int userId)
    {
        var organizer =
            await _repository
                .GetByUserIdAsync(
                    userId);


        return organizer == null
            ? null
            : Map(organizer);
    }


    // ============================================
    // ADMIN VERIFY ORGANIZER
    // ============================================

    public async Task<OrganizerResponseDto?>
        SetVerificationAsync(
            int id,
            bool isVerified)
    {
        var organizer =
            await _repository
                .GetByIdAsync(id);


        if (organizer == null)
        {
            return null;
        }


        var oldStatus =
            organizer.IsVerified;


        organizer.IsVerified =
            isVerified;


        organizer.UpdatedAt =
            DateTime.UtcNow;


        await _repository
            .SaveChangesAsync();


        // Only send notification when status changed.
        if (oldStatus != isVerified)
        {
            if (isVerified)
            {
                await _notificationService
                    .SendToUserAsync(
                        organizer.UserId,

                        "Organizer Approved",

                        "Your organizer account has been approved by the administrator.",

                        "OrganizerApproval");
            }
            else
            {
                await _notificationService
                    .SendToUserAsync(
                        organizer.UserId,

                        "Organizer Verification Updated",

                        "Your organizer account is currently not approved.",

                        "OrganizerApproval");
            }
        }


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