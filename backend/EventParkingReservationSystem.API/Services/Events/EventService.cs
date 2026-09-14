using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Events;
using EventParkingReservationSystem.API.Interfaces.Events;
using EventParkingReservationSystem.API.Models.Core;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Models.Transactions;
using EventParkingReservationSystem.API.Repositories.Events.Interfaces;
using EventParkingReservationSystem.API.Services.Transactions;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Events;

public class EventService(
    IEventRepository eventRepository,
    IEventCategoryRepository categoryRepository,
    ITicketRepository ticketRepository,
    ISeatRepository seatRepository,
    AppDbContext db,
    IEventBookingReadService? bookingReadService = null,
    IEventReferenceReadService? referenceReadService = null) : IEventService
{
    private readonly IEventRepository _events = eventRepository;
    private readonly IEventCategoryRepository _categories = categoryRepository;
    private readonly ITicketRepository _tickets = ticketRepository;
    private readonly ISeatRepository _seats = seatRepository;
    private readonly AppDbContext _db = db;
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
        ValidateEventDefinition(
            dto.Name,
            dto.TicketPrice,
            dto.StartDateTime,
            dto.EndDateTime,
            requireFutureStart: true);

        var type = ParseEventType(dto.EventType);
        var venueMode = ParseVenueMode(dto.VenueMode);
        ValidateVenueSelection(
            venueMode,
            dto.VenueId,
            dto.ExternalVenueName,
            dto.ExternalVenueAddress);

        var category = await _categories.GetByIdAsync(
            dto.EventCategoryId,
            false,
            cancellationToken);

        if (category is null || !category.IsActive)
            throw new ArgumentException("Event category does not exist or is inactive.");

        int? organizerId = IsAdmin(actorRole)
            ? dto.OrganizerId
            : actorOrganizerId
                ?? throw new UnauthorizedAccessException(
                    "Organizer account/claim is required to create an event.");

        if (_references is not null)
        {
            if (venueMode == EventVenueMode.OurProperty &&
                !await _references.VenueExistsAsync(dto.VenueId!.Value, cancellationToken))
                throw new ArgumentException("Venue does not exist.");

            if (organizerId.HasValue &&
                !await _references.OrganizerExistsAsync(organizerId.Value, cancellationToken))
                throw new ArgumentException("Organizer does not exist.");
        }

        if (venueMode == EventVenueMode.OurProperty &&
            await _events.HasVenueTimeConflictAsync(
                dto.VenueId!.Value,
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
            VenueMode = venueMode,
            ExternalVenueName = venueMode == EventVenueMode.ExternalProperty
                ? Normalize(dto.ExternalVenueName)
                : null,
            ExternalVenueAddress = venueMode == EventVenueMode.ExternalProperty
                ? Normalize(dto.ExternalVenueAddress)
                : null,
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
        ValidateEventDefinition(
            dto.Name,
            dto.TicketPrice,
            dto.StartDateTime,
            dto.EndDateTime,
            requireFutureStart: false);

        var type = ParseEventType(dto.EventType);
        var venueMode = ParseVenueMode(dto.VenueMode);
        ValidateVenueSelection(
            venueMode,
            dto.VenueId,
            dto.ExternalVenueName,
            dto.ExternalVenueAddress);
        var entity = await GetEditableEventAsync(
            id,
            actorOrganizerId,
            actorRole,
            cancellationToken);

        if (dto.StartDateTime != entity.StartDateTime &&
            dto.StartDateTime <= DateTime.UtcNow)
        {
            throw new ArgumentException(
                "A changed StartDateTime must be in the future.");
        }

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
            venueMode == EventVenueMode.OurProperty &&
            !await _references.VenueExistsAsync(dto.VenueId!.Value, cancellationToken))
        {
            throw new ArgumentException("Venue does not exist.");
        }

        if (venueMode == EventVenueMode.OurProperty &&
            await _events.HasVenueTimeConflictAsync(
                dto.VenueId!.Value,
                dto.StartDateTime,
                dto.EndDateTime,
                id,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "Another active event is already scheduled at this venue during the selected time.");
        }

        var hasActiveBookings = await HasActiveBookingsAsync(id, cancellationToken);
        var materialChanges = DescribeMaterialChanges(entity, dto, type, venueMode);

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

        if (hasActiveBookings &&
            (entity.VenueMode != venueMode ||
             entity.VenueId != dto.VenueId ||
             !string.Equals(entity.ExternalVenueName, Normalize(dto.ExternalVenueName), StringComparison.Ordinal) ||
             !string.Equals(entity.ExternalVenueAddress, Normalize(dto.ExternalVenueAddress), StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                "Venue cannot be changed after active bookings exist.");
        }

        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description.Trim();
        entity.EventType = type;
        entity.VenueId = dto.VenueId;
        entity.VenueMode = venueMode;
        entity.ExternalVenueName = venueMode == EventVenueMode.ExternalProperty
            ? Normalize(dto.ExternalVenueName)
            : null;
        entity.ExternalVenueAddress = venueMode == EventVenueMode.ExternalProperty
            ? Normalize(dto.ExternalVenueAddress)
            : null;
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

        if (hasActiveBookings && materialChanges.Count > 0)
        {
            var affectedBookings = await _db.Bookings
                .AsNoTracking()
                .Where(x => x.EventId == id && x.Status != BookingStatus.Cancelled)
                .Select(x => new { x.Id, x.CustomerId })
                .ToListAsync(cancellationToken);

            var message = $"{entity.Name} was updated: {string.Join("; ", materialChanges)}.";
            _db.Notifications.AddRange(affectedBookings.Select(booking => new Notification
            {
                CustomerId = booking.CustomerId,
                BookingId = booking.Id,
                Title = "Event updated",
                Message = message
            }));

            if (IsOrganizer(actorRole))
            {
                var adminIds = await _db.Users
                    .Where(x => x.Role == UserRole.Admin && x.IsActive)
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);
                _db.UserNotifications.AddRange(adminIds.Select(userId => new UserNotification
                {
                    UserId = userId,
                    Title = "Organizer event updated",
                    Message = message,
                    Type = "EventChange",
                    CreatedAt = DateTime.UtcNow
                }));
            }
            else if (entity.OrganizerId.HasValue)
            {
                var organizerUserId = await _db.Organizers
                    .Where(x => x.Id == entity.OrganizerId.Value)
                    .Select(x => (int?)x.UserId)
                    .SingleOrDefaultAsync(cancellationToken);
                if (organizerUserId.HasValue)
                {
                    _db.UserNotifications.Add(new UserNotification
                    {
                        UserId = organizerUserId.Value,
                        Title = "Admin updated your event",
                        Message = message,
                        Type = "EventChange",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        if (!hasActiveBookings && materialChanges.Count > 0)
        {
            var message = $"{entity.Name} was updated: {string.Join("; ", materialChanges)}.";
            if (IsOrganizer(actorRole))
            {
                var adminIds = await _db.Users
                    .Where(x => x.Role == UserRole.Admin && x.IsActive)
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);
                _db.UserNotifications.AddRange(adminIds.Select(userId => new UserNotification
                {
                    UserId = userId,
                    Title = "Organizer event updated",
                    Message = message,
                    Type = "EventChange",
                    CreatedAt = DateTime.UtcNow
                }));
            }
            else if (entity.OrganizerId.HasValue)
            {
                var organizerUserId = await _db.Organizers
                    .Where(x => x.Id == entity.OrganizerId.Value)
                    .Select(x => (int?)x.UserId)
                    .SingleOrDefaultAsync(cancellationToken);
                if (organizerUserId.HasValue)
                {
                    _db.UserNotifications.Add(new UserNotification
                    {
                        UserId = organizerUserId.Value,
                        Title = "Admin updated your event",
                        Message = message,
                        Type = "EventChange",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
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

        var publishableStatus = entity.OrganizerId.HasValue
            ? EventStatus.Approved
            : EventStatus.Draft;

        if (entity.Status != publishableStatus)
        {
            throw new InvalidOperationException(
                entity.OrganizerId.HasValue
                    ? "Only Approved organizer events can be published."
                    : "Only Draft admin-owned events can be published.");
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

        if (entity.OrganizerId.HasValue)
        {
            var organizerUserId = await _db.Organizers
                .Where(x => x.Id == entity.OrganizerId.Value)
                .Select(x => (int?)x.UserId)
                .SingleOrDefaultAsync(cancellationToken);
            if (organizerUserId.HasValue)
            {
                _db.UserNotifications.Add(new UserNotification
                {
                    UserId = organizerUserId.Value,
                    Title = "Event published",
                    Message = $"{entity.Name} is now published and visible to customers.",
                    Type = "Publication",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

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

    public async Task<EventDto> CancelAsync(
        int id,
        CancelEventDto dto,
        int actorUserId,
        int? actorOrganizerId,
        string actorRole,
        CancellationToken cancellationToken = default)
    {
        return await ReservationExecution.RunAsync(_db,
            () => CancelCoreAsync(id, dto, actorUserId, actorOrganizerId, actorRole, cancellationToken));
    }

    private async Task<EventDto> CancelCoreAsync(
        int id, CancelEventDto dto, int actorUserId, int? actorOrganizerId,
        string actorRole, CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, cancellationToken);
        await _db.Events
            .FromSqlInterpolated($"SELECT * FROM [Events] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {id}")
            .SingleOrDefaultAsync(cancellationToken);

        var entity = await GetEditableEventAsync(
            id,
            actorOrganizerId,
            actorRole,
            cancellationToken);

        if (entity.Status == EventStatus.Cancelled)
            throw new InvalidOperationException("Event is already cancelled.");

        var now = DateTime.UtcNow;
        var reason = Normalize(dto.Reason) ?? "Cancelled by event management.";

        var bookings = await _db.Bookings
            .Include(x => x.Seats)
            .Include(x => x.Parking)
            .Include(x => x.Payment)
            .Include(x => x.QrCode)
            .Where(x => x.EventId == id && x.Status != BookingStatus.Cancelled)
            .ToListAsync(cancellationToken);

        foreach (var booking in bookings)
        {
            if (booking.Seats.Count > 0)
                _db.BookingSeats.RemoveRange(booking.Seats);

            if (booking.Parking is not null)
                _db.BookingParkings.Remove(booking.Parking);

            if (booking.QrCode is not null)
                _db.QrCodes.Remove(booking.QrCode);

            if (booking.Payment is not null)
            {
                booking.Payment.Status = booking.Payment.Status == PaymentStatus.Completed
                    ? PaymentStatus.Refunded
                    : PaymentStatus.Failed;
            }

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAtUtc = now;

            _db.Notifications.Add(new Notification
            {
                CustomerId = booking.CustomerId,
                BookingId = booking.Id,
                Title = "Event cancelled",
                Message = $"{entity.Name} on {entity.StartDateTime:u} was cancelled. " +
                          $"Reason: {reason}. Booking {booking.BookingNumber} is cancelled. " +
                          (booking.Payment?.Status == PaymentStatus.Refunded
                              ? "The completed payment has been marked refunded."
                              : "No completed payment refund is due.")
            });
        }

        var pendingApprovals = await _db.EventApprovals
            .Where(x => x.EventId == id && x.Status == ApprovalStatus.Pending)
            .ToListAsync(cancellationToken);

        foreach (var approval in pendingApprovals)
        {
            approval.Status = ApprovalStatus.Rejected;
            approval.ReviewedByUserId = actorUserId;
            approval.ReviewedAt = now;
            approval.ReviewReason = reason;
        }

        entity.Status = EventStatus.Cancelled;
        entity.RejectionReason = reason;
        entity.UpdatedAt = now;

        var lifecycleMessage = $"{entity.Name} was cancelled. Reason: {reason}";
        if (IsOrganizer(actorRole))
        {
            var adminIds = await _db.Users
                .Where(x => x.Role == UserRole.Admin && x.IsActive)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
            _db.UserNotifications.AddRange(adminIds.Select(userId => new UserNotification
            {
                UserId = userId,
                Title = "Organizer event cancellation",
                Message = lifecycleMessage,
                Type = "Cancellation",
                CreatedAt = now
            }));
        }
        else if (entity.OrganizerId.HasValue)
        {
            var organizerUserId = await _db.Organizers
                .Where(x => x.Id == entity.OrganizerId.Value)
                .Select(x => (int?)x.UserId)
                .SingleOrDefaultAsync(cancellationToken);
            if (organizerUserId.HasValue)
            {
                _db.UserNotifications.Add(new UserNotification
                {
                    UserId = organizerUserId.Value,
                    Title = "Event cancelled by Admin",
                    Message = lifecycleMessage,
                    Type = "Cancellation",
                    CreatedAt = now
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

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

    private static EventVenueMode ParseVenueMode(string value) =>
        Enum.TryParse<EventVenueMode>(value, true, out var parsed) && Enum.IsDefined(parsed)
            ? parsed
            : throw new ArgumentException(
                "VenueMode must be OurProperty or ExternalProperty.");

    private static void ValidateVenueSelection(
        EventVenueMode venueMode,
        int? venueId,
        string? externalVenueName,
        string? externalVenueAddress)
    {
        if (venueMode == EventVenueMode.OurProperty && (!venueId.HasValue || venueId.Value <= 0))
            throw new ArgumentException("VenueId is required for an internal property event.");

        if (venueMode == EventVenueMode.ExternalProperty &&
            (string.IsNullOrWhiteSpace(externalVenueName) || string.IsNullOrWhiteSpace(externalVenueAddress)))
        {
            throw new ArgumentException(
                "External venue name and address are required for an external property event.");
        }
    }

    private static bool TryParseEventType(string? value, out EventType parsed) =>
        Enum.TryParse(value, true, out parsed) &&
        Enum.IsDefined(parsed);

    private static void ValidateEventDefinition(
        string? name,
        decimal ticketPrice,
        DateTime start,
        DateTime end,
        bool requireFutureStart)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Event name is required.");
        }

        if (ticketPrice < 0)
        {
            throw new ArgumentException(
                "Ticket price cannot be negative.");
        }

        if (start == default || end == default)
        {
            throw new ArgumentException(
                "StartDateTime and EndDateTime are required.");
        }

        if (end <= start)
        {
            throw new ArgumentException(
                "EndDateTime must be later than StartDateTime.");
        }

        if (requireFutureStart && start <= DateTime.UtcNow)
        {
            throw new ArgumentException(
                "StartDateTime must be in the future.");
        }
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static List<string> DescribeMaterialChanges(
        Event entity,
        UpdateEventDto dto,
        EventType type,
        EventVenueMode venueMode)
    {
        var changes = new List<string>();
        if (!string.Equals(entity.Name, dto.Name.Trim(), StringComparison.Ordinal))
            changes.Add($"name changed from '{entity.Name}' to '{dto.Name.Trim()}'");
        if (entity.StartDateTime != dto.StartDateTime)
            changes.Add($"start changed from {entity.StartDateTime:u} to {dto.StartDateTime:u}");
        if (entity.EndDateTime != dto.EndDateTime)
            changes.Add($"end changed from {entity.EndDateTime:u} to {dto.EndDateTime:u}");
        if (entity.EventType != type)
            changes.Add("entry/seat rules changed");
        if (entity.TicketPrice != dto.TicketPrice)
            changes.Add($"base price changed from {entity.TicketPrice:0.##} to {dto.TicketPrice:0.##}");
        if (entity.VenueMode != venueMode || entity.VenueId != dto.VenueId ||
            !string.Equals(entity.ExternalVenueName, Normalize(dto.ExternalVenueName), StringComparison.Ordinal) ||
            !string.Equals(entity.ExternalVenueAddress, Normalize(dto.ExternalVenueAddress), StringComparison.Ordinal))
            changes.Add("venue changed");
        return changes;
    }

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
        VenueMode = x.VenueMode.ToString(),
        ExternalVenueName = x.ExternalVenueName,
        ExternalVenueAddress = x.ExternalVenueAddress,
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
