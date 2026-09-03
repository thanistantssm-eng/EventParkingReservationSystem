using EventParkingReservationSystem.API.DTOs.Events;

namespace EventParkingReservationSystem.API.Interfaces.Events;

public interface IEventCategoryService
{
    Task<IReadOnlyList<EventCategoryDto>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<EventCategoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EventCategoryDto> CreateAsync(CreateEventCategoryDto dto, CancellationToken cancellationToken = default);
    Task<EventCategoryDto> UpdateAsync(int id, UpdateEventCategoryDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
