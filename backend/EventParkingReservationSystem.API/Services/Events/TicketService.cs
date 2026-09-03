using EventParkingReservationSystem.API.DTOs.Events;
using EventParkingReservationSystem.API.Interfaces.Events;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Repositories.Events.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Events;

public class TicketService(
    ITicketRepository ticketRepository,
    IEventRepository eventRepository,
    ISeatRepository seatRepository,
    IEventBookingReadService? bookingReadService = null) : ITicketService
{
    private readonly ITicketRepository _tickets = ticketRepository;
    private readonly IEventRepository _events = eventRepository;
    private readonly ISeatRepository _seats = seatRepository;
    private readonly IEventBookingReadService? _bookings = bookingReadService;

    public async Task<IReadOnlyList<TicketTypeDto>> GetByEventAsync(
        int eventId,
        int? actorOrganizerId,
        string? actorRole,
        CancellationToken cancellationToken = default)
    {
        var evt = await _events.GetByIdAsync(eventId, false, cancellationToken)
            ?? throw new KeyNotFoundException("Event not found.");

        EnsureCanViewEvent(evt, actorOrganizerId, actorRole);

        var list = await _tickets.Query()
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.Price)
            .ToListAsync(cancellationToken);

        return list.Select(Map).ToList();
    }

    public async Task<TicketTypeDto> CreateAsync(
        int eventId,
        CreateTicketTypeDto dto,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default)
    {
        var evt = await GetEditableEventAsync(
            eventId,
            actorOrganizerId,
            actorRole,
            cancellationToken);

        ValidateQuantity(evt.EventType, dto.Quantity);

        var name = dto.Name.Trim();

        if (await _tickets.NameExistsAsync(
                eventId,
                name,
                null,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "This ticket type name already exists for the event.");
        }

        var entity = new TicketType
        {
            EventId = eventId,
            Name = name,
            Description = Normalize(dto.Description),
            Price = dto.Price,
            Quantity = dto.Quantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _tickets.AddAsync(entity, cancellationToken);
        await _tickets.SaveChangesAsync(cancellationToken);

        return Map(entity);
    }

    public async Task<TicketTypeDto> UpdateAsync(
        int ticketTypeId,
        UpdateTicketTypeDto dto,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default)
    {
        var entity = await _tickets.GetByIdAsync(
            ticketTypeId,
            true,
            cancellationToken)
            ?? throw new KeyNotFoundException("Ticket type not found.");

        var evt = await GetEditableEventAsync(
            entity.EventId,
            actorOrganizerId,
            actorRole,
            cancellationToken);

        ValidateQuantity(evt.EventType, dto.Quantity);

        var name = dto.Name.Trim();

        if (await _tickets.NameExistsAsync(
                entity.EventId,
                name,
                ticketTypeId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "This ticket type name already exists for the event.");
        }

        entity.Name = name;
        entity.Description = Normalize(dto.Description);
        entity.Price = dto.Price;
        entity.Quantity = dto.Quantity;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _tickets.SaveChangesAsync(cancellationToken);

        return Map(entity);
    }

    public async Task DeleteAsync(
        int ticketTypeId,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default)
    {
        var entity = await _tickets.GetByIdAsync(
            ticketTypeId,
            true,
            cancellationToken)
            ?? throw new KeyNotFoundException("Ticket type not found.");

        await GetEditableEventAsync(
            entity.EventId,
            actorOrganizerId,
            actorRole,
            cancellationToken);

        if (await _seats.Query().AnyAsync(
                x => x.TicketTypeId == ticketTypeId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "Ticket type is assigned to seats. Reassign or remove those seats first.");
        }

        _tickets.Remove(entity);
        await _tickets.SaveChangesAsync(cancellationToken);
    }

    private async Task<Event> GetEditableEventAsync(
        int eventId,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken)
    {
        var evt = await _events.GetByIdAsync(
            eventId,
            false,
            cancellationToken)
            ?? throw new KeyNotFoundException("Event not found.");

        if (!EventService.IsAdmin(actorRole) &&
            !(EventService.IsOrganizer(actorRole) &&
              actorOrganizerId == evt.OrganizerId))
        {
            throw new UnauthorizedAccessException(
                "You can manage tickets only for your own event.");
        }

        if (evt.Status is EventStatus.PendingApproval
            or EventStatus.Approved
            or EventStatus.Published
            or EventStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Ticket setup is locked for this event status.");
        }

        if (_bookings is not null &&
            await _bookings.HasActiveBookingsAsync(
                eventId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "Ticket price/type setup cannot be changed after active bookings exist.");
        }

        return evt;
    }

    private static void EnsureCanViewEvent(
        Event evt,
        int? actorOrganizerId,
        string? actorRole)
    {
        if (evt.Status == EventStatus.Published)
            return;

        if (EventService.IsAdmin(actorRole))
            return;

        if (EventService.IsOrganizer(actorRole) &&
            actorOrganizerId.HasValue &&
            actorOrganizerId.Value == evt.OrganizerId)
        {
            return;
        }

        throw new UnauthorizedAccessException(
            "Tickets are not public until the event is published.");
    }

    private static void ValidateQuantity(EventType type, int quantity)
    {
        if (type == EventType.NonSeatBased && quantity <= 0)
        {
            throw new ArgumentException(
                "NonSeatBased ticket quantity must be greater than zero.");
        }

        if (quantity < 0)
            throw new ArgumentException("Ticket quantity cannot be negative.");
    }

    private static TicketTypeDto Map(TicketType x) => new()
    {
        Id = x.Id,
        EventId = x.EventId,
        Name = x.Name,
        Description = x.Description,
        Price = x.Price,
        Quantity = x.Quantity,
        IsActive = x.IsActive
    };

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
