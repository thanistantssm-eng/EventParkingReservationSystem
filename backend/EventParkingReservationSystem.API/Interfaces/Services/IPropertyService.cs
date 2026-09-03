using EventParkingReservationSystem.API.DTOs.Properties;

namespace EventParkingReservationSystem.API.Interfaces.Services;

public interface IPropertyService
{
    Task<List<PropertyDto>> GetAllAsync();

    Task<PropertyDto> GetByIdAsync(int id);

    Task<PropertyDto> CreateAsync(
        CreatePropertyDto request);

    Task<PropertyDto> UpdateAsync(
        int id,
        UpdatePropertyDto request);

    Task DeactivateAsync(int id);
}