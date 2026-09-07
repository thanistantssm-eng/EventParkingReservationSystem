using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Venues;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Core;

public class VenueService : IVenueService
{
    private readonly AppDbContext _context;

    public VenueService(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<VenueDto>>
        GetAllAsync(
            int? propertyId = null)
    {
        var query =
            _context.Venues
                .Include(x => x.Property)
                .AsNoTracking()
                .AsQueryable();

        if (propertyId.HasValue)
        {
            query = query.Where(
                x =>
                    x.PropertyId ==
                    propertyId.Value);
        }

        var list =
            await query
                .OrderBy(x => x.Name)
                .ToListAsync();

        return list.Select(Map).ToList();
    }

    public async Task<VenueDto?>
        GetByIdAsync(int id)
    {
        var venue =
            await _context.Venues
                .Include(x => x.Property)
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        return venue == null
            ? null
            : Map(venue);
    }

    public async Task<VenueDto>
        CreateAsync(
            CreateVenueDto request)
    {
        var property =
            await _context.Properties
                .FirstOrDefaultAsync(
                    x =>
                        x.Id ==
                        request.PropertyId &&
                        x.IsActive);

        if (property == null)
        {
            throw new InvalidOperationException(
                "Active property not found.");
        }

        var name = request.Name.Trim();

        var duplicate =
            await _context.Venues
                .AnyAsync(x =>
                    x.PropertyId ==
                    request.PropertyId &&
                    x.Name.ToLower() ==
                    name.ToLower());

        if (duplicate)
        {
            throw new InvalidOperationException(
                "Venue already exists in this property.");
        }

        var venue = new Venue
        {
            PropertyId =
                request.PropertyId,

            Property = property,

            Name = name,

            Location =
                Clean(request.Location),

            Capacity =
                request.Capacity,

            IsActive = true,

            CreatedAt =
                DateTime.UtcNow
        };

        await _context.Venues.AddAsync(
            venue);

        await _context.SaveChangesAsync();

        return Map(venue);
    }

    public async Task<VenueDto?>
        UpdateAsync(
            int id,
            UpdateVenueDto request)
    {
        var venue =
            await _context.Venues
                .Include(x => x.Property)
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        if (venue == null)
        {
            return null;
        }

        var property =
            await _context.Properties
                .FirstOrDefaultAsync(
                    x =>
                        x.Id ==
                        request.PropertyId &&
                        x.IsActive);

        if (property == null)
        {
            throw new InvalidOperationException(
                "Active property not found.");
        }

        var name = request.Name.Trim();

        var duplicate =
            await _context.Venues
                .AnyAsync(x =>
                    x.Id != id &&
                    x.PropertyId ==
                    request.PropertyId &&
                    x.Name.ToLower() ==
                    name.ToLower());

        if (duplicate)
        {
            throw new InvalidOperationException(
                "Venue already exists in this property.");
        }

        venue.PropertyId =
            request.PropertyId;

        venue.Property = property;

        venue.Name = name;

        venue.Location =
            Clean(request.Location);

        venue.Capacity =
            request.Capacity;

        venue.IsActive =
            request.IsActive;

        venue.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Map(venue);
    }

    public async Task<bool> DeleteAsync(
        int id)
    {
        var venue =
            await _context.Venues
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        if (venue == null)
        {
            return false;
        }

        _context.Venues.Remove(venue);

        await _context.SaveChangesAsync();

        return true;
    }

    private static VenueDto Map(
        Venue venue)
    {
        return new VenueDto
        {
            Id = venue.Id,

            PropertyId =
                venue.PropertyId,

            PropertyName =
                venue.Property?.Name
                ?? string.Empty,

            Name = venue.Name,

            Location =
                venue.Location,

            Capacity =
                venue.Capacity,

            IsActive =
                venue.IsActive,

            CreatedAt =
                venue.CreatedAt,

            UpdatedAt =
                venue.UpdatedAt
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