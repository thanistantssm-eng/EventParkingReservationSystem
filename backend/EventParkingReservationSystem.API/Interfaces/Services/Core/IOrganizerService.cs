using EventParkingReservationSystem.API.DTOs.Organizers;

namespace EventParkingReservationSystem.API.Interfaces.Services.Core;

public interface IOrganizerService
{
    Task<List<OrganizerResponseDto>> GetAllAsync();

    Task<OrganizerResponseDto?> GetByIdAsync(
        int id);

    Task<OrganizerResponseDto?> GetByUserIdAsync(
        int userId);

    Task<OrganizerResponseDto?> SetVerificationAsync(
        int id,
        bool isVerified);
}