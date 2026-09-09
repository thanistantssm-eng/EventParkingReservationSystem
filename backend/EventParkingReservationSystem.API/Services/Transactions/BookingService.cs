using System.Data;
using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Middleware;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Models.Transactions;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Transactions;

public sealed class BookingService(AppDbContext db, IBookingExpiryService expiry) : IBookingService
{
    public async Task<BookingDto> CreateAsync(
        CreateBookingDto request,
        CancellationToken ct)
    {
        await expiry.ExpireStalePendingBookingsAsync(ct);

        if (request.SeatIds.Count !=
            request.SeatIds.Distinct().Count())
        {
            throw new ValidationException(
                "Duplicate seats are not allowed.");
        }

        await using var tx =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                ct);

        var customerExists = await db.Customers
            .AnyAsync(x => x.Id == request.CustomerId, ct);

        if (!customerExists)
        {
            throw new NotFoundException(
                "Customer not found.");
        }

        var eventInfo = await db.Events
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == request.EventId,
                ct)
            ?? throw new NotFoundException(
                "Event not found.");

        if (eventInfo.Status != EventStatus.Published)
        {
            throw new ValidationException(
                "Only published events can be booked.");
        }

        if (eventInfo.StartDateTime <= DateTime.UtcNow)
        {
            throw new ValidationException(
                "Past events cannot be booked.");
        }

        var requestedTicketName =
            request.TicketType.Trim();

        var ticketType = await db.TicketTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x =>
                    x.EventId == request.EventId &&
                    x.IsActive &&
                    x.Name == requestedTicketName,
                ct);

        if (eventInfo.EventType == EventType.NonSeatBased &&
            ticketType is null)
        {
            throw new ValidationException(
                "A valid active ticket type is required for a non-seat-based event.");
        }

        var seats = await ValidateSeatsAsync(
            eventInfo,
            request,
            ticketType,
            ct);

        await ValidateTicketCapacityAsync(
            eventInfo,
            request,
            ticketType,
            ct);

        var parking =
            await ResolveParkingAsync(
                request.EventId,
                request.ParkingSlotId,
                ct);

        var ticketTotal = CalculateTicketTotal(
            eventInfo,
            seats,
            ticketType,
            request.Quantity);

        var totalAmount =
            ticketTotal + parking.Fee;

        var unitPrice =
            request.Quantity == 0
                ? 0
                : ticketTotal / request.Quantity;

        var booking = new Booking
        {
            BookingNumber =
                $"BKG-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",

            CustomerId = request.CustomerId,
            EventId = request.EventId,
            TotalAmount = totalAmount,

            Tickets =
            [
                new BookingTicket
                {
                    TicketType =
                        ticketType?.Name ??
                        requestedTicketName,

                    Quantity = request.Quantity,
                    UnitPrice = unitPrice
                }
            ],

            Seats = seats
                .Select(x =>
                    new BookingSeat
                    {
                        SeatId = x.Id
                    })
                .ToList(),

            Parking = parking.SlotId.HasValue
                ? new BookingParking
                {
                    ParkingSlotId =
                        parking.SlotId.Value,

                    Fee = parking.Fee
                }
                : null
        };

        db.Bookings.Add(booking);

        db.Notifications.Add(
            new Notification
            {
                CustomerId = request.CustomerId,
                Title = "Booking created",
                Message =
                    "Your booking is awaiting payment."
            });

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return await GetAsync(
            booking.Id,
            ct);
    }

    public async Task<BookingDto> GetAsync(
        int id,
        CancellationToken ct)
    {
        await expiry.ExpireStalePendingBookingsAsync(ct);

        return Map(
            await Query()
                .SingleOrDefaultAsync(
                    x => x.Id == id,
                    ct)
            ?? throw new NotFoundException(
                "Booking not found."));
    }

    public async Task<IReadOnlyList<BookingDto>>
        GetCustomerBookingsAsync(
            int customerId,
            CancellationToken ct)
    {
        await expiry.ExpireStalePendingBookingsAsync(ct);

        return (await Query()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct))
        .Select(Map)
        .ToList();
    }

    public async Task<IReadOnlyList<BookingDto>>
        GetEventBookingsAsync(
            int eventId,
            CancellationToken ct)
    {
        await expiry.ExpireStalePendingBookingsAsync(ct);

        return (await Query()
            .Where(x => x.EventId == eventId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct))
        .Select(Map)
        .ToList();
    }

    public async Task<EventAvailabilityDto>
        GetAvailabilityAsync(
            int eventId,
            CancellationToken ct)
    {
        await expiry.ExpireStalePendingBookingsAsync(ct);

        var item = await db.Events
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == eventId,
                ct)
            ?? throw new NotFoundException(
                "Event not found.");

        var takenSeats =
            (await db.BookingSeats
                .AsNoTracking()
                .Where(x =>
                    x.Booking.EventId == eventId &&
                    x.Booking.Status !=
                        BookingStatus.Cancelled)
                .Select(x => x.SeatId)
                .ToListAsync(ct))
            .ToHashSet();

        var seats = item.EventType ==
                    EventType.SeatBased
            ? await db.Seats
                .AsNoTracking()
                .Where(x =>
                    x.EventId == eventId &&
                    x.IsActive &&
                    x.SetupStatus ==
                        SeatSetupStatus.Available)
                .OrderBy(x => x.SeatNumber)
                .ToListAsync(ct)
            : [];

        var allocations =
            await db.EventParkingAllocations
                .AsNoTracking()
                .Include(x => x.ParkingArea)
                    .ThenInclude(x => x!.Slots)
                .Where(x =>
                    x.EventId == eventId &&
                    x.IsActive)
                .OrderBy(x => x.ParkingAreaId)
                .ToListAsync(ct);

        var allowedParkingSlots =
            new List<ParkingSlot>();

        foreach (var allocation in allocations)
        {
            var activeSlots =
                allocation.ParkingArea?.Slots
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.SlotNumber)
                    .ToList()
                ?? [];

            if (allocation.AllocatedSlotCount > 0)
            {
                activeSlots = activeSlots
                    .Take(
                        allocation
                            .AllocatedSlotCount)
                    .ToList();
            }

            allowedParkingSlots.AddRange(
                activeSlots);
        }

        var allowedParkingIds =
            allowedParkingSlots
                .Select(x => x.Id)
                .ToList();

        var takenParking =
            allowedParkingIds.Count == 0
                ? new HashSet<int>()
                : (await db.BookingParkings
                    .AsNoTracking()
                    .Where(x =>
                        allowedParkingIds.Contains(
                            x.ParkingSlotId) &&
                        x.Booking.Status !=
                            BookingStatus.Cancelled)
                    .Select(x => x.ParkingSlotId)
                    .ToListAsync(ct))
                .ToHashSet();

        var displayParkingFee =
            allocations.Count == 0
                ? 0m
                : allocations.Min(
                    x => x.ParkingFee);

        return new EventAvailabilityDto(
            item.Id,
            item.Name,
            item.TicketPrice,
            displayParkingFee,

            seats
                .Select(x =>
                    new SelectionItemDto(
                        x.Id,
                        x.SeatNumber,
                        takenSeats.Contains(x.Id)))
                .ToList(),

            allowedParkingSlots
                .OrderBy(x => x.SlotNumber)
                .Select(x =>
                    new SelectionItemDto(
                        x.Id,
                        x.SlotNumber,
                        takenParking.Contains(x.Id)))
                .ToList());
    }

    public async Task<BookingDto> AttachParkingAsync(
        int id,
        int customerId,
        ReserveParkingDto request,
        CancellationToken ct)
    {
        await expiry.ExpireStalePendingBookingsAsync(ct);

        await using var tx =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                ct);

        var booking = await db.Bookings
            .Include(x => x.Parking)
            .Include(x => x.Payment)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                ct)
            ?? throw new NotFoundException(
                "Booking not found.");

        if (booking.CustomerId != customerId)
        {
            throw new ApiException(
                403,
                "You can only modify parking for your own booking.");
        }

        if (booking.Status != BookingStatus.PendingPayment)
        {
            throw new ConflictException(
                "Parking can only be changed before the booking is finalized.");
        }

        if (booking.Payment is not null)
        {
            throw new ConflictException(
                "Parking cannot be changed after payment has started.");
        }

        if (booking.Parking is not null)
        {
            throw new ConflictException(
                "This booking already has a parking reservation.");
        }

        var parking = await ResolveParkingAsync(
            booking.EventId,
            request.ParkingSlotId,
            ct);

        if (!parking.SlotId.HasValue)
        {
            throw new ValidationException(
                "A parking slot is required.");
        }

        booking.Parking = new BookingParking
        {
            ParkingSlotId = parking.SlotId.Value,
            Fee = parking.Fee
        };

        booking.TotalAmount += parking.Fee;

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return await GetAsync(id, ct);
    }

    public async Task RemoveParkingAsync(
        int id,
        int customerId,
        CancellationToken ct)
    {
        await expiry.ExpireStalePendingBookingsAsync(ct);

        await using var tx =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                ct);

        var booking = await db.Bookings
            .Include(x => x.Parking)
            .Include(x => x.Payment)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                ct)
            ?? throw new NotFoundException(
                "Booking not found.");

        if (booking.CustomerId != customerId)
        {
            throw new ApiException(
                403,
                "You can only modify parking for your own booking.");
        }

        if (booking.Status != BookingStatus.PendingPayment)
        {
            throw new ConflictException(
                "Parking can only be changed before the booking is finalized.");
        }

        if (booking.Payment is not null)
        {
            throw new ConflictException(
                "Parking cannot be changed after payment has started.");
        }

        if (booking.Parking is null)
        {
            throw new NotFoundException(
                "This booking does not have a parking reservation.");
        }

        var parkingFee = booking.Parking.Fee;
        db.BookingParkings.Remove(booking.Parking);
        booking.TotalAmount = Math.Max(0m, booking.TotalAmount - parkingFee);

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task CancelAsync(
        int id,
        int customerId,
        CancellationToken ct)
    {
        await expiry.ExpireStalePendingBookingsAsync(ct);

        await using var tx =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                ct);

        var booking = await db.Bookings
            .Include(x => x.Seats)
            .Include(x => x.Parking)
            .Include(x => x.Payment)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                ct)
            ?? throw new NotFoundException(
                "Booking not found.");

        if (booking.CustomerId != customerId)
        {
            throw new ApiException(
                403,
                "You can only cancel your own booking.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new ConflictException(
                "Booking is already cancelled or expired.");
        }

        if (booking.Payment?.Status == PaymentStatus.Completed)
        {
            throw new ConflictException(
                "A completed booking cannot be cancelled directly. An administrator must refund the payment first.");
        }

        if (booking.Payment is not null &&
            booking.Payment.Status == PaymentStatus.PendingOtp)
        {
            booking.Payment.Status = PaymentStatus.Failed;
        }

        db.BookingSeats.RemoveRange(
            booking.Seats);

        if (booking.Parking is not null)
        {
            db.BookingParkings.Remove(
                booking.Parking);
        }

        booking.Status =
            BookingStatus.Cancelled;

        booking.CancelledAtUtc =
            DateTime.UtcNow;

        db.Notifications.Add(
            new Notification
            {
                CustomerId = customerId,
                BookingId = id,
                Title = "Booking cancelled",
                Message =
                    $"Booking {booking.BookingNumber} was cancelled."
            });

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    private async Task<List<Seat>> ValidateSeatsAsync(
        Event eventInfo,
        CreateBookingDto request,
        TicketType? ticketType,
        CancellationToken ct)
    {
        if (eventInfo.EventType ==
            EventType.NonSeatBased)
        {
            if (request.SeatIds.Count > 0)
            {
                throw new ValidationException(
                    "Seats cannot be selected for a non-seat-based event.");
            }

            return [];
        }

        if (request.SeatIds.Count == 0)
        {
            throw new ValidationException(
                "At least one seat is required for a seat-based event.");
        }

        if (request.Quantity !=
            request.SeatIds.Count)
        {
            throw new ValidationException(
                "Ticket quantity must equal the selected seat count for a seat-based event.");
        }

        var seats = await db.Seats
            .AsNoTracking()
            .Include(x => x.TicketType)
            .Where(x =>
                request.SeatIds.Contains(x.Id) &&
                x.EventId == request.EventId &&
                x.IsActive &&
                x.SetupStatus ==
                    SeatSetupStatus.Available)
            .ToListAsync(ct);

        if (seats.Count !=
            request.SeatIds.Count)
        {
            throw new ValidationException(
                "Every selected seat must be active, available, and belong to the event.");
        }

        if (ticketType is not null &&
            seats.Any(x =>
                x.TicketTypeId.HasValue &&
                x.TicketTypeId.Value !=
                    ticketType.Id))
        {
            throw new ValidationException(
                "One or more selected seats do not belong to the selected ticket type.");
        }

        var takenSeats = await db.BookingSeats
            .Where(x =>
                request.SeatIds.Contains(
                    x.SeatId) &&
                x.Booking.Status !=
                    BookingStatus.Cancelled)
            .Select(x => x.SeatId)
            .ToListAsync(ct);

        if (takenSeats.Count > 0)
        {
            throw new ConflictException(
                $"Seat(s) already booked: {string.Join(", ", takenSeats)}.");
        }

        return seats;
    }

    private async Task ValidateTicketCapacityAsync(
        Event eventInfo,
        CreateBookingDto request,
        TicketType? ticketType,
        CancellationToken ct)
    {
        if (eventInfo.EventType !=
                EventType.NonSeatBased ||
            ticketType is null)
        {
            return;
        }

        if (ticketType.Quantity <= 0)
        {
            throw new ConflictException(
                "The selected ticket type has no available capacity.");
        }

        var soldQuantity = await db.BookingTickets
            .Where(x =>
                x.Booking.EventId ==
                    request.EventId &&
                x.Booking.Status !=
                    BookingStatus.Cancelled &&
                x.TicketType ==
                    ticketType.Name)
            .SumAsync(
                x => (int?)x.Quantity,
                ct)
            ?? 0;

        if (soldQuantity + request.Quantity >
            ticketType.Quantity)
        {
            throw new ConflictException(
                "Requested ticket quantity exceeds the remaining ticket capacity.");
        }
    }

    private async Task<ParkingSelection>
        ResolveParkingAsync(
            int eventId,
            int? parkingSlotId,
            CancellationToken ct)
    {
        if (!parkingSlotId.HasValue)
        {
            return new ParkingSelection(
                null,
                0m);
        }

        var slot = await db.ParkingSlots
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x =>
                    x.Id ==
                        parkingSlotId.Value &&
                    x.IsActive,
                ct)
            ?? throw new ValidationException(
                "The selected parking slot is invalid or inactive.");

        var allocation =
            await db.EventParkingAllocations
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x =>
                        x.EventId == eventId &&
                        x.ParkingAreaId ==
                            slot.ParkingAreaId &&
                        x.IsActive,
                    ct)
            ?? throw new ValidationException(
                "The parking slot is not allocated to this event.");

        if (allocation.AllocatedSlotCount > 0)
        {
            var allocatedIds =
                await db.ParkingSlots
                    .AsNoTracking()
                    .Where(x =>
                        x.ParkingAreaId ==
                            slot.ParkingAreaId &&
                        x.IsActive)
                    .OrderBy(x => x.SlotNumber)
                    .Take(
                        allocation
                            .AllocatedSlotCount)
                    .Select(x => x.Id)
                    .ToListAsync(ct);

            if (!allocatedIds.Contains(
                    slot.Id))
            {
                throw new ValidationException(
                    "The parking slot is outside this event's allocated slot range.");
            }
        }

        var alreadyReserved =
            await db.BookingParkings
                .AnyAsync(
                    x =>
                        x.ParkingSlotId ==
                            slot.Id &&
                        x.Booking.Status !=
                            BookingStatus.Cancelled,
                    ct);

        if (alreadyReserved)
        {
            throw new ConflictException(
                "The selected parking slot is already reserved.");
        }

        return new ParkingSelection(
            slot.Id,
            allocation.ParkingFee);
    }

    private static decimal CalculateTicketTotal(
        Event eventInfo,
        IReadOnlyCollection<Seat> seats,
        TicketType? ticketType,
        int quantity)
    {
        if (eventInfo.EventType ==
            EventType.NonSeatBased)
        {
            var unitPrice =
                ticketType?.Price ??
                eventInfo.TicketPrice;

            return unitPrice * quantity;
        }

        return seats.Sum(
            seat =>
                seat.PriceOverride ??
                seat.TicketType?.Price ??
                ticketType?.Price ??
                eventInfo.TicketPrice);
    }

    private IQueryable<Booking> Query() =>
        db.Bookings
            .AsNoTracking()
            .Include(x => x.Event)
            .Include(x => x.Seats)
                .ThenInclude(x => x.Seat)
            .Include(x => x.Parking)
                .ThenInclude(x =>
                    x!.ParkingSlot)
            .Include(x => x.Payment);

    private static BookingDto Map(
        Booking x) =>
        new(
            x.Id,
            x.BookingNumber,
            x.CustomerId,
            x.EventId,
            x.Event.Name,
            x.Status.ToString(),
            x.TotalAmount,
            x.Seats
                .Select(s =>
                    s.Seat.SeatNumber)
                .ToList(),
            x.Parking?
                .ParkingSlot
                .SlotNumber,
            x.Payment?
                .Status
                .ToString()
                ?? "NotStarted",
            x.CreatedAtUtc);

    private sealed record ParkingSelection(
        int? SlotId,
        decimal Fee);
}
