using EventParkingReservationSystem.API.DTOs.Events;

namespace EventParkingReservationSystem.API.Interfaces.Events;

public interface ISeatService
{
    Task<IReadOnlyList<SeatDto>> GetByEventAsync(
        int eventId,
        int? actorOrganizerId,
        string? actorRole,
        CancellationToken cancellationToken = default);

    Task<SeatDto> GetByIdAsync(
        int seatId,
        int? actorOrganizerId,
        string? actorRole,
        CancellationToken cancellationToken = default);

    Task<SeatDto> CreateAsync(
        int eventId,
        CreateSeatDto dto,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SeatDto>> CreateBulkAsync(
        int eventId,
        IReadOnlyCollection<CreateSeatDto> dtos,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default);

    Task<SeatDto> UpdateAsync(
        int seatId,
        UpdateSeatDto dto,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int seatId,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default);
}
