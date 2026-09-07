using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Properties;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Core;

public class PropertyService : IPropertyService
{
    private readonly AppDbContext _context;

    public PropertyService(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PropertyDto>>
        GetAllAsync()
    {
        return await _context.Properties
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new PropertyDto
            {
                Id = x.Id,

                Name = x.Name,

                Address = x.Address,

                City = x.City,

                Description = x.Description,

                IsActive = x.IsActive,

                CreatedAt = x.CreatedAt,

                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<PropertyDto?>
        GetByIdAsync(int id)
    {
        var property =
            await _context.Properties
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        return property == null
            ? null
            : Map(property);
    }

    public async Task<PropertyDto>
        CreateAsync(
            CreatePropertyDto request)
    {
        var name = request.Name.Trim();

        var exists =
            await _context.Properties
                .AnyAsync(x =>
                    x.Name.ToLower() ==
                    name.ToLower());

        if (exists)
        {
            throw new InvalidOperationException(
                "Property already exists.");
        }

        var property = new Property
        {
            Name = name,

            Address =
                request.Address.Trim(),

            City =
                Clean(request.City),

            Description =
                Clean(request.Description),

            IsActive = true,

            CreatedAt = DateTime.UtcNow
        };

        await _context.Properties
            .AddAsync(property);

        await _context.SaveChangesAsync();

        return Map(property);
    }

    public async Task<PropertyDto?>
        UpdateAsync(
            int id,
            UpdatePropertyDto request)
    {
        var property =
            await _context.Properties
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        if (property == null)
        {
            return null;
        }

        var name = request.Name.Trim();

        var duplicate =
            await _context.Properties
                .AnyAsync(x =>
                    x.Id != id &&
                    x.Name.ToLower() ==
                    name.ToLower());

        if (duplicate)
        {
            throw new InvalidOperationException(
                "Property already exists.");
        }

        property.Name = name;

        property.Address =
            request.Address.Trim();

        property.City =
            Clean(request.City);

        property.Description =
            Clean(request.Description);

        property.IsActive =
            request.IsActive;

        property.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Map(property);
    }

    public async Task<bool> DeleteAsync(
        int id)
    {
        var property =
            await _context.Properties
                .Include(x => x.Venues)
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        if (property == null)
        {
            return false;
        }

        if (property.Venues.Any())
        {
            throw new InvalidOperationException(
                "Delete the property's venues first.");
        }

        _context.Properties.Remove(property);

        await _context.SaveChangesAsync();

        return true;
    }

    private static PropertyDto Map(
        Property property)
    {
        return new PropertyDto
        {
            Id = property.Id,

            Name = property.Name,

            Address = property.Address,

            City = property.City,

            Description =
                property.Description,

            IsActive =
                property.IsActive,

            CreatedAt =
                property.CreatedAt,

            UpdatedAt =
                property.UpdatedAt
        };
    }

    private static string? Clean(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}