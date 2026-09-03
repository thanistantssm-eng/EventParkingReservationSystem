using EventParkingReservationSystem.API.DTOs.Events;

namespace EventParkingReservationSystem.API.Interfaces.Events;

public interface ITicketService
{
    Task<IReadOnlyList<TicketTypeDto>> GetByEventAsync(int eventId, int? actorOrganizerId, string? actorRole, CancellationToken cancellationToken = default);
    Task<TicketTypeDto> CreateAsync(int eventId, CreateTicketTypeDto dto, int? actorOrganizerId, string actorRole, CancellationToken cancellationToken = default);
    Task<TicketTypeDto> UpdateAsync(int ticketTypeId, UpdateTicketTypeDto dto, int? actorOrganizerId, string actorRole, CancellationToken cancellationToken = default);
    Task DeleteAsync(int ticketTypeId, int? actorOrganizerId, string actorRole, CancellationToken cancellationToken = default);
}
