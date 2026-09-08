using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Interfaces.Events;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Core;

public class EventReferenceReadService : IEventReferenceReadService
{
    private readonly AppDbContext _context;

    public EventReferenceReadService(
        AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> VenueExistsAsync(
        int venueId,
        CancellationToken cancellationToken = default)
    {
        return _context.Venues
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == venueId &&
                    x.IsActive &&
                    x.Property.IsActive,
                cancellationToken);
    }

    public Task<bool> OrganizerExistsAsync(
        int organizerId,
        CancellationToken cancellationToken = default)
    {
        return _context.Organizers
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == organizerId &&
                    x.IsVerified &&
                    x.User.IsActive,
                cancellationToken);
    }
}