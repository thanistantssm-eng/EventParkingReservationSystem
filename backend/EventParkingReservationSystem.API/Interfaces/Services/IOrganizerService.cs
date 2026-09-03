using EventParkingReservationSystem.API.DTOs.Organizers;

namespace EventParkingReservationSystem.API.Interfaces.Services;

public interface IOrganizerService
{
    Task<List<OrganizerDto>> GetAllAsync();

    Task<OrganizerDto> GetByIdAsync(int id);

    Task<OrganizerDto> CreateAsync(
        CreateOrganizerDto request);

    Task<OrganizerDto> UpdateAsync(
        int id,
        UpdateOrganizerDto request);

    Task DeactivateAsync(int id);
}