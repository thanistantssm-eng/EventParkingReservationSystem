using EventParkingReservationSystem.API.DTOs.Events;
using EventParkingReservationSystem.API.Interfaces.Events;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Repositories.Events.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Events;

public class EventCategoryService(IEventCategoryRepository repository) : IEventCategoryService
{
    private readonly IEventCategoryRepository _repository = repository;

    public async Task<IReadOnlyList<EventCategoryDto>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _repository.Query();
        if (!includeInactive) query = query.Where(x => x.IsActive);
        var list = await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
        return list.Select(Map).ToList();
    }

    public async Task<EventCategoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, false, cancellationToken)
            ?? throw new KeyNotFoundException("Event category not found.");
        return Map(entity);
    }

    public async Task<EventCategoryDto> CreateAsync(CreateEventCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var name = dto.Name.Trim();
        if (await _repository.NameExistsAsync(name, null, cancellationToken))
            throw new InvalidOperationException("An event category with this name already exists.");

        var entity = new EventCategory
        {
            Name = name,
            Description = Normalize(dto.Description),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<EventCategoryDto> UpdateAsync(int id, UpdateEventCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, true, cancellationToken)
            ?? throw new KeyNotFoundException("Event category not found.");

        var name = dto.Name.Trim();
        if (await _repository.NameExistsAsync(name, id, cancellationToken))
            throw new InvalidOperationException("An event category with this name already exists.");

        entity.Name = name;
        entity.Description = Normalize(dto.Description);
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, true, cancellationToken)
            ?? throw new KeyNotFoundException("Event category not found.");

        if (await _repository.IsUsedByAnyEventAsync(id, cancellationToken))
            throw new InvalidOperationException("This category is used by an active event and cannot be deleted.");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static EventCategoryDto Map(EventCategory x) => new() { Id = x.Id, Name = x.Name, Description = x.Description, IsActive = x.IsActive };
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
