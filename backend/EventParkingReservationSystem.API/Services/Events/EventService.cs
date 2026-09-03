using EventParkingReservationSystem.API.DTOs.Events;
using EventParkingReservationSystem.API.Interfaces.Events;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Repositories.Events.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Events;

public class EventService(
    IEventRepository eventRepository,
    IEventCategoryRepository categoryRepository,
    ITicketRepository ticketRepository,
    ISeatRepository seatRepository,
    IEventBookingReadService? bookingReadService = null,
    IEventReferenceReadService? referenceReadService = null) : IEventService
{
    private readonly IEventRepository _events = eventRepository;
    private readonly IEventCategoryRepository _categories = categoryRepository;
    private readonly ITicketRepository _tickets = ticketRepository;
    private readonly ISeatRepository _seats = seatRepository;
    private readonly IEventBookingReadService? _bookings = bookingReadService;
    private readonly IEventReferenceReadService? _references = referenceReadService;

    public async Task<IReadOnlyList<EventDto>> GetAllAsync(
        EventQueryDto query,
        int? actorUserId,
        int? actorOrganizerId,
        string? actorRole,
        CancellationToken cancellationToken = default)
    {
        var items = _events.Query()
            .Include(x => x.EventCategory)
            .AsQueryable();

        if (!IsAdmin(actorRole))
        {
            if (IsOrganizer(actorRole) && actorOrganizerId.HasValue)
            {
                items = items.Where(x =>
                    x.Status == EventStatus.Published ||
                    x.OrganizerId == actorOrganizerId.Value);
            }
            else
            {
                items = items.Where(x => x.Status == EventStatus.Published);
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            items = items.Where(x =>
                x.Name.Contains(search) ||
                x.Description.Contains(search));
        }

        if (query.Date.HasValue)
        {
            var start = query.Date.Value.Date;
            var end = start.AddDays(1);
            items = items.Where(x =>
                x.StartDateTime >= start &&
                x.StartDateTime < end);
        }

        var venueId = query.Venue ?? query.VenueId;
        var categoryId = query.Category ?? query.EventCategoryId;

        if (venueId.HasValue)
            items = items.Where(x => x.VenueId == venueId.Value);

        if (categoryId.HasValue)
            items = items.Where(x => x.EventCategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(query.EventType) &&
            TryParseEventType(query.EventType, out var type))
        {
            items = items.Where(x => x.EventType == type);
        }

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            IsAdmin(actorRole) &&
            Enum.TryParse<EventStatus>(query.Status, true, out var status))
        {
            items = items.Where(x => x.Status == status);
        }

        var list = await items
            .OrderBy(x => x.StartDateTime)
            .ToListAsync(cancellationToken);

        return list.Select(Map).ToList();
    }

    public async Task<EventDto> GetByIdAsync(
        int id,
        int? actorUserId,
        int? actorOrganizerId,
        string? actorRole,
        CancellationToken cancellationToken = default)
    {
        var entity = await _events.Query()
            .Include(x => x.EventCategory)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Event not found.");

        EnsureCanView(entity, actorOrganizerId, actorRole);
        return Map(entity);
    }

    public async Task<EventDto> GetByQrCodeAsync(
        string qrCode,
        int? actorOrganizerId,
        string? actorRole,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(qrCode))
            throw new ArgumentException("QR code is required.");

        var normalized = qrCode.Trim();

        var entity = await _events.Query()
            .Include(x => x.EventCategory)
            .FirstOrDefaultAsync(x => x.EventQrCode == normalized, cancellationToken)
            ?? throw new KeyNotFoundException("Event QR code was not found.");

        EnsureCanView(entity, actorOrganizerId, actorRole);
        return Map(entity);
    }

    public async Task<EventDto> CreateAsync(
        CreateEventDto dto,
        int actorUserId,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(dto.StartDateTime, dto.EndDateTime);
        var type = ParseEventType(dto.EventType);

        var category = await _categories.GetByIdAsync(
            dto.EventCategoryId,
            false,
            cancellationToken);

        if (category is null || !category.IsActive)
            throw new ArgumentException("Event category does not exist or is inactive.");

        var organizerId = IsAdmin(actorRole)
            ? dto.OrganizerId
                ?? throw new ArgumentException(
                    "OrganizerId is required when Admin creates an event for an organizer.")
            : actorOrganizerId
                ?? throw new UnauthorizedAccessException(
                    "Organizer account/claim is required to create an event.");

        if (_references is not null)
        {
            if (!await _references.VenueExistsAsync(dto.VenueId, cancellationToken))
                throw new ArgumentException("Venue does not exist.");

            if (!await _references.OrganizerExistsAsync(organizerId, cancellationToken))
                throw new ArgumentException("Organizer does not exist.");
        }

        if (await _events.HasVenueTimeConflictAsync(
                dto.VenueId,
                dto.StartDateTime,
                dto.EndDateTime,
                null,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "Another active event is already scheduled at this venue during the selected time.");
        }

        var entity = new Event
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            EventType = type,
            OrganizerId = organizerId,
            VenueId = dto.VenueId,
            EventCategoryId = dto.EventCategoryId,
            StartDateTime = dto.StartDateTime,
            EndDateTime = dto.EndDateTime,
            TicketPrice = dto.TicketPrice,
            PosterUrl = Normalize(dto.PosterUrl),
            Status = EventStatus.Draft,
            CreatedByUserId = actorUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _events.AddAsync(entity, cancellationToken);
        await _events.SaveChangesAsync(cancellationToken);

        entity.EventQrCode = GenerateEventQr(entity.Id);
        await _events.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            entity.Id,
            actorUserId,
            organizerId,
            actorRole,
            cancellationToken);
    }

    public async Task<EventDto> UpdateAsync(
        int id,
        UpdateEventDto dto,
        int actorUserId,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(dto.StartDateTime, dto.EndDateTime);
        var type = ParseEventType(dto.EventType);
        var entity = await GetEditableEventAsync(
            id,
            actorOrganizerId,
            actorRole,
            cancellationToken);

        if (entity.Status is EventStatus.PendingApproval or EventStatus.Approved)
        {
            throw new InvalidOperationException(
                "Pending or approved events cannot be edited. Admin must reject/reopen them first.");
        }

        if (entity.Status == EventStatus.Published && !IsAdmin(actorRole))
            throw new InvalidOperationException("Only Admin can edit a published event.");

        if (entity.Status == EventStatus.Cancelled)
            throw new InvalidOperationException("Cancelled events cannot be edited.");

        var category = await _categories.GetByIdAsync(
            dto.EventCategoryId,
            false,
            cancellationToken);

        if (category is null || !category.IsActive)
            throw new ArgumentException("Event category does not exist or is inactive.");

        if (_references is not null &&
            !await _references.VenueExistsAsync(dto.VenueId, cancellationToken))
        {
            throw new ArgumentException("Venue does not exist.");
        }

        if (await _events.HasVenueTimeConflictAsync(
                dto.VenueId,
                dto.StartDateTime,
                dto.EndDateTime,
                id,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "Another active event is already scheduled at this venue during the selected time.");
        }

        var hasActiveBookings = await HasActiveBookingsAsync(id, cancellationToken);

        if (hasActiveBookings && entity.TicketPrice != dto.TicketPrice)
        {
            throw new InvalidOperationException(
                "Ticket price cannot be changed after active bookings exist.");
        }

        if (hasActiveBookings && entity.EventType != type)
        {
            throw new InvalidOperationException(
                "Event type cannot be changed after active bookings exist.");
        }

        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description.Trim();
        entity.EventType = type;
        entity.VenueId = dto.VenueId;
        entity.EventCategoryId = dto.EventCategoryId;
        entity.StartDateTime = dto.StartDateTime;
        entity.EndDateTime = dto.EndDateTime;
        entity.TicketPrice = dto.TicketPrice;
        entity.PosterUrl = Normalize(dto.PosterUrl);
        entity.UpdatedAt = DateTime.UtcNow;

        if (entity.Status == EventStatus.Rejected)
        {
            entity.Status = EventStatus.Draft;
            entity.RejectionReason = null;
        }

        await _events.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            id,
            actorUserId,
            actorOrganizerId,
            actorRole,
            cancellationToken);
    }

    public async Task DeleteAsync(
        int id,
        int actorUserId,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetEditableEventAsync(
            id,
            actorOrganizerId,
            actorRole,
            cancellationToken);

        if (await HasActiveBookingsAsync(id, cancellationToken))
        {
            throw new InvalidOperationException(
                "Event cannot be deleted while active bookings exist.");
        }

        // BRD: Admin may delete an event only when it has no active bookings.
        // Organizer is an enhanced role, so organizer deletion remains limited
        // to its own Draft/Rejected events.
        if (!IsAdmin(actorRole) &&
            entity.Status is not (EventStatus.Draft or EventStatus.Rejected))
        {
            throw new InvalidOperationException(
                "Organizers can delete only Draft or Rejected events.");
        }

        _events.Remove(entity);
        await _events.SaveChangesAsync(cancellationToken);
    }

    public async Task<EventDto> PublishAsync(
        int id,
        int actorUserId,
        string actorRole,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin(actorRole))
            throw new UnauthorizedAccessException("Only Admin can publish events.");

        var entity = await _events.GetByIdAsync(id, true, cancellationToken)
            ?? throw new KeyNotFoundException("Event not found.");

        if (entity.Status is not (EventStatus.Approved or EventStatus.Draft))
        {
            throw new InvalidOperationException(
                "Only Approved events or Admin-created Draft events can be published.");
        }

        var activeTickets = await _tickets.Query()
            .Where(x => x.EventId == id && x.IsActive)
            .ToListAsync(cancellationToken);

        if (activeTickets.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one active ticket type is required before publishing.");
        }

        if (entity.EventType == EventType.SeatBased)
        {
            var hasSeats = await _seats.Query()
                .AnyAsync(
                    x => x.EventId == id && x.IsActive,
                    cancellationToken);

            if (!hasSeats)
            {
                throw new InvalidOperationException(
                    "SeatBased events must have at least one active seat before publishing.");
            }
        }
        else if (activeTickets.Sum(x => x.Quantity) <= 0)
        {
            throw new InvalidOperationException(
                "NonSeatBased events must have a ticket quantity greater than zero before publishing.");
        }

        entity.Status = EventStatus.Published;
        entity.PublishedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _events.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            id,
            actorUserId,
            null,
            actorRole,
            cancellationToken);
    }

    public async Task<EventDto> RegenerateQrAsync(
        int id,
        int actorUserId,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetEditableEventAsync(
            id,
            actorOrganizerId,
            actorRole,
            cancellationToken);

        entity.EventQrCode = GenerateEventQr(entity.Id);
        entity.UpdatedAt = DateTime.UtcNow;

        await _events.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            id,
            actorUserId,
            actorOrganizerId,
            actorRole,
            cancellationToken);
    }

    private async Task<Event> GetEditableEventAsync(
        int id,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken)
    {
        var entity = await _events.GetByIdAsync(
            id,
            true,
            cancellationToken)
            ?? throw new KeyNotFoundException("Event not found.");

        if (!IsAdmin(actorRole) &&
            !(IsOrganizer(actorRole) && actorOrganizerId == entity.OrganizerId))
        {
            throw new UnauthorizedAccessException(
                "You can manage only your own events.");
        }

        return entity;
    }

    private async Task<bool> HasActiveBookingsAsync(
        int eventId,
        CancellationToken cancellationToken)
    {
        return _bookings is not null &&
               await _bookings.HasActiveBookingsAsync(eventId, cancellationToken);
    }

    private static void EnsureCanView(
        Event entity,
        int? actorOrganizerId,
        string? actorRole)
    {
        if (entity.Status == EventStatus.Published)
            return;

        if (IsAdmin(actorRole))
            return;

        if (IsOrganizer(actorRole) &&
            actorOrganizerId.HasValue &&
            actorOrganizerId.Value == entity.OrganizerId)
        {
            return;
        }

        throw new UnauthorizedAccessException(
            "This event is not available to the current user.");
    }

    internal static bool IsAdmin(string? role) =>
        string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

    internal static bool IsOrganizer(string? role) =>
        string.Equals(role, "Organizer", StringComparison.OrdinalIgnoreCase);

    private static EventType ParseEventType(string value) =>
        TryParseEventType(value, out var parsed)
            ? parsed
            : throw new ArgumentException(
                "EventType must be SeatBased or NonSeatBased.");

    private static bool TryParseEventType(string? value, out EventType parsed) =>
        Enum.TryParse(value, true, out parsed) &&
        Enum.IsDefined(parsed);

    private static void ValidateDateRange(DateTime start, DateTime end)
    {
        if (start == default || end == default || end <= start)
        {
            throw new ArgumentException(
                "EndDateTime must be later than StartDateTime.");
        }
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string GenerateEventQr(int eventId) =>
        $"EVT-{eventId}-{Guid.NewGuid():N}";

    private static EventDto Map(Event x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Description = x.Description,
        EventType = x.EventType.ToString(),
        OrganizerId = x.OrganizerId,
        VenueId = x.VenueId,
        EventCategoryId = x.EventCategoryId,
        EventCategoryName = x.EventCategory?.Name,
        StartDateTime = x.StartDateTime,
        EndDateTime = x.EndDateTime,
        TicketPrice = x.TicketPrice,
        Status = x.Status.ToString(),
        PosterUrl = x.PosterUrl,
        EventQrCode = x.EventQrCode,
        RejectionReason = x.RejectionReason,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
        ApprovedAt = x.ApprovedAt,
        PublishedAt = x.PublishedAt
    };
}
