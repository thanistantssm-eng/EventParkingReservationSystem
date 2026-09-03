using EventParkingReservationSystem.API.DTOs.Events;

namespace EventParkingReservationSystem.API.Interfaces.Events;

public interface IEventService
{
    Task<IReadOnlyList<EventDto>> GetAllAsync(EventQueryDto query, int? actorUserId, int? actorOrganizerId, string? actorRole, CancellationToken cancellationToken = default);
    Task<EventDto> GetByIdAsync(int id, int? actorUserId, int? actorOrganizerId, string? actorRole, CancellationToken cancellationToken = default);
    Task<EventDto> GetByQrCodeAsync(string qrCode, int? actorOrganizerId, string? actorRole, CancellationToken cancellationToken = default);
    Task<EventDto> CreateAsync(CreateEventDto dto, int actorUserId, int? actorOrganizerId, string actorRole, CancellationToken cancellationToken = default);
    Task<EventDto> UpdateAsync(int id, UpdateEventDto dto, int actorUserId, int? actorOrganizerId, string actorRole, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, int actorUserId, int? actorOrganizerId, string actorRole, CancellationToken cancellationToken = default);
    Task<EventDto> PublishAsync(int id, int actorUserId, string actorRole, CancellationToken cancellationToken = default);
    Task<EventDto> RegenerateQrAsync(int id, int actorUserId, int? actorOrganizerId, string actorRole, CancellationToken cancellationToken = default);
}
