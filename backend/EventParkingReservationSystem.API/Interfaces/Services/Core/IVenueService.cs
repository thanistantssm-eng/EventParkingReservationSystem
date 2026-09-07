using EventParkingReservationSystem.API.DTOs.Venues;

namespace EventParkingReservationSystem.API.Interfaces.Services.Core;

public interface IVenueService
{
    Task<List<VenueDto>> GetAllAsync(int? propertyId = null);

    Task<VenueDto?> GetByIdAsync(int id);

    Task<VenueDto> CreateAsync(CreateVenueDto request);

    Task<VenueDto?> UpdateAsync(
        int id,
        UpdateVenueDto request);

    Task<bool> DeleteAsync(int id);
}