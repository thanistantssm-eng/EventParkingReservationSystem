using EventParkingReservationSystem.API.DTOs.Properties;
using EventParkingReservationSystem.API.Exceptions;
using EventParkingReservationSystem.API.Interfaces.Repositories;
using EventParkingReservationSystem.API.Interfaces.Services;
using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.Services;

public class PropertyService : IPropertyService
{
    private readonly IPropertyRepository _repository;

    public PropertyService(
        IPropertyRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PropertyDto>> GetAllAsync()
    {
        var properties =
            await _repository.GetAllAsync();

        return properties
            .Select(Map)
            .ToList();
    }

    public async Task<PropertyDto> GetByIdAsync(int id)
    {
        var property =
            await _repository.GetByIdAsync(id);

        if (property is null)
        {
            throw new AppException(
                "Property not found.",
                StatusCodes.Status404NotFound);
        }

        return Map(property);
    }

    public async Task<PropertyDto> CreateAsync(
        CreatePropertyDto request)
    {
        var property = new Property
        {
            Name = request.Name.Trim(),

            Location = request.Location.Trim(),

            Capacity = request.Capacity,

            IsActive = true
        };

        await _repository.AddAsync(property);

        return Map(property);
    }

    public async Task<PropertyDto> UpdateAsync(
        int id,
        UpdatePropertyDto request)
    {
        var property =
            await _repository.GetByIdAsync(id);

        if (property is null)
        {
            throw new AppException(
                "Property not found.",
                StatusCodes.Status404NotFound);
        }

        property.Name =
            request.Name.Trim();

        property.Location =
            request.Location.Trim();

        property.Capacity =
            request.Capacity;

        property.IsActive =
            request.IsActive;

        await _repository.SaveChangesAsync();

        return Map(property);
    }

    public async Task DeactivateAsync(int id)
    {
        var property =
            await _repository.GetByIdAsync(id);

        if (property is null)
        {
            throw new AppException(
                "Property not found.",
                StatusCodes.Status404NotFound);
        }

        property.IsActive = false;

        await _repository.SaveChangesAsync();
    }

    private static PropertyDto Map(
        Property property)
    {
        return new PropertyDto
        {
            Id = property.Id,
            Name = property.Name,
            Location = property.Location,
            Capacity = property.Capacity,
            IsActive = property.IsActive
        };
    }
}