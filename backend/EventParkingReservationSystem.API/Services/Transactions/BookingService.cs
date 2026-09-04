using System.Data;
using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Middleware;
using EventParkingReservationSystem.API.Models.Transactions;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Transactions;

public sealed class BookingService(AppDbContext db) : IBookingService
{
    public async Task<BookingDto> CreateAsync(CreateBookingDto request, CancellationToken ct)
    {
        if (request.SeatIds.Count == 0) throw new ValidationException("At least one seat is required.");
        if (request.SeatIds.Count != request.SeatIds.Distinct().Count()) throw new ValidationException("Duplicate seats are not allowed.");
        if (request.Quantity != request.SeatIds.Count) throw new ValidationException("Ticket quantity must equal the selected seat count.");

        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var customerExists = await db.Customers.AnyAsync(x => x.Id == request.CustomerId, ct);
        if (!customerExists) throw new NotFoundException("Customer not found.");
        var eventInfo = await db.Events.SingleOrDefaultAsync(x => x.Id == request.EventId, ct)
            ?? throw new NotFoundException("Event not found.");
        if (eventInfo.StartsAtUtc <= DateTime.UtcNow) throw new ValidationException("Past events cannot be booked.");

        var seats = await db.Seats.Where(x => request.SeatIds.Contains(x.Id) && x.EventId == request.EventId).ToListAsync(ct);
        if (seats.Count != request.SeatIds.Count) throw new ValidationException("Every selected seat must belong to the event.");
        var takenSeats = await db.BookingSeats.Where(x => request.SeatIds.Contains(x.SeatId)).Select(x => x.SeatId).ToListAsync(ct);
        if (takenSeats.Count > 0) throw new ConflictException($"Seat(s) already booked: {string.Join(", ", takenSeats)}.");

        if (request.ParkingSlotId is int parkingId)
        {
            if (!await db.ParkingSlots.AnyAsync(x => x.Id == parkingId && x.EventId == request.EventId, ct))
                throw new ValidationException("The parking slot does not belong to the event.");
            if (await db.BookingParkings.AnyAsync(x => x.ParkingSlotId == parkingId, ct))
                throw new ConflictException("The selected parking slot is already reserved.");
        }

        var booking = new Booking
        {
            BookingNumber = $"BKG-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            CustomerId = request.CustomerId,
            EventId = request.EventId,
            TotalAmount = eventInfo.TicketPrice * request.Quantity + (request.ParkingSlotId.HasValue ? eventInfo.ParkingFee : 0),
            Tickets = [new BookingTicket { TicketType = request.TicketType.Trim(), Quantity = request.Quantity, UnitPrice = eventInfo.TicketPrice }],
            Seats = request.SeatIds.Select(id => new BookingSeat { SeatId = id }).ToList(),
            Parking = request.ParkingSlotId is int slotId ? new BookingParking { ParkingSlotId = slotId, Fee = eventInfo.ParkingFee } : null
        };
        db.Bookings.Add(booking);
        db.Notifications.Add(new Notification { CustomerId = request.CustomerId, Title = "Booking created", Message = "Your booking is awaiting payment." });
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await GetAsync(booking.Id, ct);
    }

    public async Task<BookingDto> GetAsync(int id, CancellationToken ct) => Map(await Query().SingleOrDefaultAsync(x => x.Id == id, ct)
        ?? throw new NotFoundException("Booking not found."));

    public async Task<IReadOnlyList<BookingDto>> GetCustomerBookingsAsync(int customerId, CancellationToken ct) =>
        (await Query().Where(x => x.CustomerId == customerId).OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct)).Select(Map).ToList();

    public async Task<IReadOnlyList<BookingDto>> GetEventBookingsAsync(int eventId, CancellationToken ct) =>
        (await Query().Where(x => x.EventId == eventId).OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct)).Select(Map).ToList();

    public async Task<EventAvailabilityDto> GetAvailabilityAsync(int eventId, CancellationToken ct)
    {
        var item = await db.Events.AsNoTracking().Include(x => x.Seats).Include(x => x.ParkingSlots)
            .SingleOrDefaultAsync(x => x.Id == eventId, ct) ?? throw new NotFoundException("Event not found.");
        var takenSeats = (await db.BookingSeats.AsNoTracking().Where(x => x.Booking.EventId == eventId).Select(x => x.SeatId).ToListAsync(ct)).ToHashSet();
        var takenParking = (await db.BookingParkings.AsNoTracking().Where(x => x.Booking.EventId == eventId).Select(x => x.ParkingSlotId).ToListAsync(ct)).ToHashSet();
        return new(item.Id, item.Name, item.TicketPrice, item.ParkingFee,
            item.Seats.OrderBy(x => x.SeatNumber).Select(x => new SelectionItemDto(x.Id, x.SeatNumber, takenSeats.Contains(x.Id))).ToList(),
            item.ParkingSlots.OrderBy(x => x.SlotNumber).Select(x => new SelectionItemDto(x.Id, x.SlotNumber, takenParking.Contains(x.Id))).ToList());
    }

    public async Task CancelAsync(int id, int customerId, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var booking = await db.Bookings.Include(x => x.Seats).Include(x => x.Parking).SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Booking not found.");
        if (booking.CustomerId != customerId) throw new ApiException(403, "You can only cancel your own booking.");
        db.BookingSeats.RemoveRange(booking.Seats);
        if (booking.Parking is not null) db.BookingParkings.Remove(booking.Parking);
        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAtUtc = DateTime.UtcNow;
        db.Notifications.Add(new Notification { CustomerId = customerId, BookingId = id, Title = "Booking cancelled", Message = $"Booking {booking.BookingNumber} was cancelled." });
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    private IQueryable<Booking> Query() => db.Bookings.AsNoTracking().Include(x => x.Event).Include(x => x.Seats).ThenInclude(x => x.Seat)
        .Include(x => x.Parking).ThenInclude(x => x!.ParkingSlot).Include(x => x.Payment);

    private static BookingDto Map(Booking x) => new(x.Id, x.BookingNumber, x.CustomerId, x.EventId, x.Event.Name,
        x.Status.ToString(), x.TotalAmount, x.Seats.Select(s => s.Seat.SeatNumber).ToList(), x.Parking?.ParkingSlot.SlotNumber,
        x.Payment?.Status.ToString() ?? "NotStarted", x.CreatedAtUtc);
}
