using EventParkingReservationSystem.API.DTOs.Events;
using EventParkingReservationSystem.API.Interfaces.Events;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Repositories.Events.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Events;

public class SeatService(
    ISeatRepository seatRepository,
    IEventRepository eventRepository,
    ITicketRepository ticketRepository,
    IEventBookingReadService? bookingReadService = null) : ISeatService
{
    private readonly ISeatRepository _seats = seatRepository;
    private readonly IEventRepository _events = eventRepository;
    private readonly ITicketRepository _tickets = ticketRepository;
    private readonly IEventBookingReadService? _bookings = bookingReadService;

    public async Task<IReadOnlyList<SeatDto>> GetByEventAsync(
        int eventId,
        int? actorOrganizerId,
        string? actorRole,
        CancellationToken cancellationToken = default)
    {
        var evt = await _events.GetByIdAsync(eventId, false, cancellationToken)
            ?? throw new KeyNotFoundException("Event not found.");

        EnsureCanViewEvent(evt, actorOrganizerId, actorRole);

        var bookedIds = _bookings is null
            ? new HashSet<int>()
            : (await _bookings.GetBookedSeatIdsAsync(eventId, cancellationToken))
                .ToHashSet();

        var list = await _seats.Query()
            .Include(x => x.TicketType)
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.RowLabel)
            .ThenBy(x => x.ColumnNumber)
            .ThenBy(x => x.SeatNumber)
            .ToListAsync(cancellationToken);

        return list
            .Select(x => Map(x, evt.TicketPrice, bookedIds))
            .ToList();
    }

    public async Task<SeatDto> CreateAsync(
        int eventId,
        CreateSeatDto dto,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default)
    {
        var evt = await GetEditableSeatEventAsync(
            eventId,
            actorOrganizerId,
            actorRole,
            cancellationToken);

        await ValidateTicketAsync(
            eventId,
            dto.TicketTypeId,
            cancellationToken);

        var seatNumber = dto.SeatNumber.Trim();

        if (await _seats.SeatNumberExistsAsync(
                eventId,
                seatNumber,
                null,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "This seat number already exists for the event.");
        }

        var entity = new Seat
        {
            EventId = evt.Id,
            TicketTypeId = dto.TicketTypeId,
            SeatNumber = seatNumber,
            RowLabel = Normalize(dto.RowLabel),
            ColumnNumber = dto.ColumnNumber,
            PriceOverride = dto.PriceOverride,
            SetupStatus = SeatSetupStatus.Available,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _seats.AddAsync(entity, cancellationToken);
        await _seats.SaveChangesAsync(cancellationToken);

        if (entity.TicketTypeId.HasValue)
        {
            entity.TicketType = await _tickets.GetByIdAsync(
                entity.TicketTypeId.Value,
                false,
                cancellationToken);
        }

        return Map(entity, evt.TicketPrice, new HashSet<int>());
    }

    public async Task<IReadOnlyList<SeatDto>> CreateBulkAsync(
        int eventId,
        IReadOnlyCollection<CreateSeatDto> dtos,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default)
    {
        if (dtos.Count == 0)
            throw new ArgumentException("At least one seat is required.");

        var evt = await GetEditableSeatEventAsync(
            eventId,
            actorOrganizerId,
            actorRole,
            cancellationToken);

        var normalizedNumbers = dtos
            .Select(x => x.SeatNumber.Trim())
            .ToList();

        if (normalizedNumbers.Any(string.IsNullOrWhiteSpace) ||
            normalizedNumbers
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() != normalizedNumbers.Count)
        {
            throw new ArgumentException(
                "Seat numbers must be non-empty and unique within the request.");
        }

        foreach (var dto in dtos)
        {
            await ValidateTicketAsync(
                eventId,
                dto.TicketTypeId,
                cancellationToken);

            if (await _seats.SeatNumberExistsAsync(
                    eventId,
                    dto.SeatNumber.Trim(),
                    null,
                    cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Seat '{dto.SeatNumber}' already exists for this event.");
            }
        }

        var created = new List<Seat>();

        foreach (var dto in dtos)
        {
            var entity = new Seat
            {
                EventId = eventId,
                TicketTypeId = dto.TicketTypeId,
                SeatNumber = dto.SeatNumber.Trim(),
                RowLabel = Normalize(dto.RowLabel),
                ColumnNumber = dto.ColumnNumber,
                PriceOverride = dto.PriceOverride,
                SetupStatus = SeatSetupStatus.Available,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _seats.AddAsync(entity, cancellationToken);
            created.Add(entity);
        }

        await _seats.SaveChangesAsync(cancellationToken);

        var ticketIds = created
            .Where(x => x.TicketTypeId.HasValue)
            .Select(x => x.TicketTypeId!.Value)
            .Distinct()
            .ToList();

        var tickets = ticketIds.Count == 0
            ? new Dictionary<int, TicketType>()
            : await _tickets.Query()
                .Where(x => ticketIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        foreach (var seat in created)
        {
            if (seat.TicketTypeId.HasValue &&
                tickets.TryGetValue(seat.TicketTypeId.Value, out var ticket))
            {
                seat.TicketType = ticket;
            }
        }

        return created
            .Select(x => Map(x, evt.TicketPrice, new HashSet<int>()))
            .ToList();
    }

    public async Task<SeatDto> UpdateAsync(
        int seatId,
        UpdateSeatDto dto,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default)
    {
        var entity = await _seats.GetByIdAsync(
            seatId,
            true,
            cancellationToken)
            ?? throw new KeyNotFoundException("Seat not found.");

        var evt = await GetEditableSeatEventAsync(
            entity.EventId,
            actorOrganizerId,
            actorRole,
            cancellationToken);

        await ValidateTicketAsync(
            entity.EventId,
            dto.TicketTypeId,
            cancellationToken);

        var seatNumber = dto.SeatNumber.Trim();

        if (await _seats.SeatNumberExistsAsync(
                entity.EventId,
                seatNumber,
                seatId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "This seat number already exists for the event.");
        }

        if (!Enum.TryParse<SeatSetupStatus>(
                dto.SetupStatus,
                true,
                out var setupStatus))
        {
            throw new ArgumentException(
                "SetupStatus must be Available or Disabled.");
        }

        entity.TicketTypeId = dto.TicketTypeId;
        entity.SeatNumber = seatNumber;
        entity.RowLabel = Normalize(dto.RowLabel);
        entity.ColumnNumber = dto.ColumnNumber;
        entity.PriceOverride = dto.PriceOverride;
        entity.SetupStatus = setupStatus;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _seats.SaveChangesAsync(cancellationToken);

        if (entity.TicketTypeId.HasValue)
        {
            entity.TicketType = await _tickets.GetByIdAsync(
                entity.TicketTypeId.Value,
                false,
                cancellationToken);
        }

        return Map(entity, evt.TicketPrice, new HashSet<int>());
    }

    public async Task DeleteAsync(
        int seatId,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default)
    {
        var entity = await _seats.GetByIdAsync(
            seatId,
            true,
            cancellationToken)
            ?? throw new KeyNotFoundException("Seat not found.");

        await GetEditableSeatEventAsync(
            entity.EventId,
            actorOrganizerId,
            actorRole,
            cancellationToken);

        _seats.Remove(entity);
        await _seats.SaveChangesAsync(cancellationToken);
    }

    private async Task<Event> GetEditableSeatEventAsync(
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

        if (evt.EventType != EventType.SeatBased)
        {
            throw new InvalidOperationException(
                "Seats can only be configured for SeatBased events.");
        }

        if (!EventService.IsAdmin(actorRole) &&
            !(EventService.IsOrganizer(actorRole) &&
              actorOrganizerId == evt.OrganizerId))
        {
            throw new UnauthorizedAccessException(
                "You can manage seats only for your own event.");
        }

        if (evt.Status is EventStatus.PendingApproval
            or EventStatus.Approved
            or EventStatus.Published
            or EventStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Seat setup is locked for this event status.");
        }

        if (_bookings is not null &&
            await _bookings.HasActiveBookingsAsync(
                eventId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "Seat map cannot be changed after active bookings exist.");
        }

        return evt;
    }

    private async Task ValidateTicketAsync(
        int eventId,
        int? ticketTypeId,
        CancellationToken cancellationToken)
    {
        if (!ticketTypeId.HasValue)
            return;

        var ticket = await _tickets.GetByIdAsync(
            ticketTypeId.Value,
            false,
            cancellationToken);

        if (ticket is null || ticket.EventId != eventId)
        {
            throw new ArgumentException(
                "TicketTypeId does not belong to this event.");
        }
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
            "Seat map is not public until the event is published.");
    }

    private static SeatDto Map(
        Seat x,
        decimal eventTicketPrice,
        HashSet<int> bookedIds)
    {
        var status = !x.IsActive || x.SetupStatus == SeatSetupStatus.Disabled
            ? "Disabled"
            : bookedIds.Contains(x.Id)
                ? "Booked"
                : "Available";

        var price = x.PriceOverride
                    ?? x.TicketType?.Price
                    ?? eventTicketPrice;

        return new SeatDto
        {
            Id = x.Id,
            EventId = x.EventId,
            TicketTypeId = x.TicketTypeId,
            SeatNumber = x.SeatNumber,
            RowLabel = x.RowLabel,
            ColumnNumber = x.ColumnNumber,
            PriceOverride = x.PriceOverride,
            Price = price,
            SetupStatus = x.SetupStatus.ToString(),
            Status = status,
            IsActive = x.IsActive
        };
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
