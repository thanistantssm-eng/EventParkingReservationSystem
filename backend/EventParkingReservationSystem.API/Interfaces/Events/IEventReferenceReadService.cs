namespace EventParkingReservationSystem.API.Interfaces.Events;

/// <summary>
/// Cross-module validation contract implemented by Member 1 after Core/Venue/Organizer code is merged.
/// It lets Member 2 validate foreign keys without editing Member 1 models or services.
/// </summary>
public interface IEventReferenceReadService
{
    Task<bool> VenueExistsAsync(int venueId, CancellationToken cancellationToken = default);
    Task<bool> OrganizerExistsAsync(int organizerId, CancellationToken cancellationToken = default);
}
