using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Models.Core;
using EventParkingReservationSystem.API.Models.Transactions;
using Microsoft.EntityFrameworkCore;
using EventParkingReservationSystem.API.Middleware;

namespace EventParkingReservationSystem.API.Services.Transactions;

public sealed class ReportService(AppDbContext db, IEmailService emailService) : IReportService
{
    public async Task<AdminReportDto> GetAdminSummaryAsync(
        CancellationToken ct) =>
        new(
            await db.Bookings
                .IgnoreQueryFilters()
                .CountAsync(ct),

            await db.Bookings
                .CountAsync(
                    x =>
                        x.Status ==
                        BookingStatus.Confirmed,
                    ct),

            await db.Bookings
                .IgnoreQueryFilters()
                .CountAsync(
                    x =>
                        x.Status ==
                        BookingStatus.Cancelled,
                    ct),

            await db.BookingSeats.CountAsync(ct),

            await db.BookingParkings.CountAsync(ct),

            await db.Payments
                .Where(
                    x =>
                        x.Status ==
                        PaymentStatus.Completed)
                .SumAsync(
                    x => (decimal?)x.Amount,
                    ct)
            ?? 0);

    public async Task<CustomerReportDto>
        GetCustomerSummaryAsync(
            int customerId,
            CancellationToken ct) =>
        new(
            customerId,

            await db.Bookings
                .IgnoreQueryFilters()
                .CountAsync(
                    x =>
                        x.CustomerId ==
                        customerId,
                    ct),

            await db.Bookings
                .CountAsync(
                    x =>
                        x.CustomerId ==
                            customerId &&
                        x.Event.StartDateTime >
                            DateTime.UtcNow,
                    ct),

            await db.Payments
                .Where(
                    x =>
                        x.Booking.CustomerId ==
                            customerId &&
                        x.Status ==
                            PaymentStatus.Completed)
                .SumAsync(
                    x => (decimal?)x.Amount,
                    ct)
            ?? 0,

            await db.Notifications
                .CountAsync(
                    x =>
                        x.CustomerId ==
                            customerId &&
                        !x.IsRead,
                    ct));


    public async Task<OrganizerTicketSalesDto>
    GetOrganizerTicketSalesAsync(
        int userId,
        CancellationToken ct)
    {
        var organizer =
            await db.Organizers
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.UserId == userId,
                    ct)
            ?? throw new NotFoundException(
                "Organizer profile not found.");

        var events =
            await db.Events
                .AsNoTracking()
                .Where(x =>
                    x.OrganizerId == organizer.Id)
                .OrderByDescending(x =>
                    x.StartDateTime)
                .Select(x => new
                {
                    x.Id,
                    x.Name
                })
                .ToListAsync(ct);

        if (events.Count == 0)
        {
            return new OrganizerTicketSalesDto(
                organizer.Id,
                0,
                0m,
                []);
        }

        var eventIds =
            events.Select(x => x.Id).ToList();

        var bookingCounts =
            await db.Bookings
                .AsNoTracking()
                .Where(x =>
                    eventIds.Contains(x.EventId) &&
                    x.Status == BookingStatus.Confirmed &&
                    x.Payment != null &&
                    x.Payment.Status ==
                        PaymentStatus.Completed)
                .GroupBy(x => x.EventId)
                .Select(g => new
                {
                    EventId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync(ct);

        var sales =
            await db.BookingTickets
                .AsNoTracking()
                .Where(x =>
                    eventIds.Contains(
                        x.Booking.EventId) &&
                    x.Booking.Status ==
                        BookingStatus.Confirmed &&
                    x.Booking.Payment != null &&
                    x.Booking.Payment.Status ==
                        PaymentStatus.Completed)
                .GroupBy(x => new
                {
                    x.Booking.EventId,
                    x.TicketType
                })
                .Select(g => new
                {
                    g.Key.EventId,
                    g.Key.TicketType,

                    TicketsSold =
                        g.Sum(x => x.Quantity),

                    Revenue =
                        g.Sum(x =>
                            x.Quantity *
                            x.UnitPrice)
                })
                .ToListAsync(ct);

        var eventResults =
            events.Select(eventItem =>
            {
                var eventSales =
                    sales
                        .Where(x =>
                            x.EventId ==
                            eventItem.Id)
                        .ToList();

                var confirmedBookings =
                    bookingCounts
                        .FirstOrDefault(x =>
                            x.EventId ==
                            eventItem.Id)
                        ?.Count ?? 0;

                var ticketTypes =
                    eventSales
                        .Select(x =>
                            new TicketTypeSalesDto(
                                x.TicketType,
                                x.TicketsSold,
                                x.Revenue))
                        .ToList();

                return new EventTicketSalesDto(
                    eventItem.Id,
                    eventItem.Name,
                    confirmedBookings,
                    eventSales.Sum(x =>
                        x.TicketsSold),
                    eventSales.Sum(x =>
                        x.Revenue),
                    ticketTypes);
            })
            .ToList();

        return new OrganizerTicketSalesDto(
            organizer.Id,
            eventResults.Sum(x =>
                x.TicketsSold),
            eventResults.Sum(x =>
                x.TicketRevenue),
            eventResults);
    }


    public async Task<OrganizerEventRevenueDto>
        GetOrganizerEventRevenueAsync(
            int userId,
            int eventId,
            CancellationToken ct)
    {
        var organizer =
            await db.Organizers
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.UserId == userId,
                    ct)
            ?? throw new NotFoundException(
                "Organizer profile not found.");

        var eventItem =
            await db.Events
                .AsNoTracking()
                .Where(x => x.Id == eventId)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.OrganizerId
                })
                .SingleOrDefaultAsync(ct)
            ?? throw new NotFoundException(
                "Event not found.");

        if (eventItem.OrganizerId !=
            organizer.Id)
        {
            throw new ApiException(
                403,
                "You can only view revenue for your own events.");
        }

        var confirmedBookings =
            await db.Bookings
                .AsNoTracking()
                .CountAsync(x =>
                    x.EventId == eventId &&
                    x.Status ==
                        BookingStatus.Confirmed &&
                    x.Payment != null &&
                    x.Payment.Status ==
                        PaymentStatus.Completed,
                    ct);

        var ticketsSold =
            await db.BookingTickets
                .AsNoTracking()
                .Where(x =>
                    x.Booking.EventId ==
                        eventId &&
                    x.Booking.Status ==
                        BookingStatus.Confirmed &&
                    x.Booking.Payment != null &&
                    x.Booking.Payment.Status ==
                        PaymentStatus.Completed)
                .SumAsync(
                    x => (int?)x.Quantity,
                    ct)
            ?? 0;

        var ticketRevenue =
            await db.BookingTickets
                .AsNoTracking()
                .Where(x =>
                    x.Booking.EventId ==
                        eventId &&
                    x.Booking.Status ==
                        BookingStatus.Confirmed &&
                    x.Booking.Payment != null &&
                    x.Booking.Payment.Status ==
                        PaymentStatus.Completed)
                .SumAsync(
                    x => (decimal?)
                        (x.Quantity *
                         x.UnitPrice),
                    ct)
            ?? 0m;

        var parkingReservations =
            await db.BookingParkings
                .AsNoTracking()
                .CountAsync(x =>
                    x.Booking.EventId ==
                        eventId &&
                    x.Booking.Status ==
                        BookingStatus.Confirmed &&
                    x.Booking.Payment != null &&
                    x.Booking.Payment.Status ==
                        PaymentStatus.Completed,
                    ct);

        var parkingRevenue =
            await db.BookingParkings
                .AsNoTracking()
                .Where(x =>
                    x.Booking.EventId ==
                        eventId &&
                    x.Booking.Status ==
                        BookingStatus.Confirmed &&
                    x.Booking.Payment != null &&
                    x.Booking.Payment.Status ==
                        PaymentStatus.Completed)
                .SumAsync(
                    x => (decimal?)x.Fee,
                    ct)
            ?? 0m;

        var totalRevenue =
            await db.Payments
                .AsNoTracking()
                .Where(x =>
                    x.Booking.EventId ==
                        eventId &&
                    x.Booking.Status ==
                        BookingStatus.Confirmed &&
                    x.Status ==
                        PaymentStatus.Completed)
                .SumAsync(
                    x => (decimal?)x.Amount,
                    ct)
            ?? 0m;

        return new OrganizerEventRevenueDto(
            eventItem.Id,
            eventItem.Name,
            confirmedBookings,
            ticketsSold,
            parkingReservations,
            ticketRevenue,
            parkingRevenue,
            totalRevenue);
    }

    public async Task<EventReportDto> GetOrganizerEventReportAsync(
        int userId,
        int eventId,
        CancellationToken ct)
    {
        var organizer = await db.Organizers
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == userId, ct)
            ?? throw new NotFoundException("Organizer profile not found.");

        return await BuildEventReportAsync(eventId, organizer.Id, ct);
    }

    public Task<EventReportDto> GetAdminOrganizerEventReportAsync(
        int organizerId,
        int eventId,
        CancellationToken ct) =>
        BuildEventReportAsync(eventId, organizerId, ct);

    public async Task<EventReportDto> SendAdminOrganizerEventReportAsync(
        int organizerId,
        int eventId,
        CancellationToken ct)
    {
        var report = await BuildEventReportAsync(eventId, organizerId, ct);
        if (string.IsNullOrWhiteSpace(report.OrganizerEmail))
            throw new InvalidOperationException("The organizer does not have a registered email address.");

        var summary = string.Join('\n', new[]
        {
            $"Generated: {report.GeneratedAtUtc:u}",
            $"Status: {report.EventStatus}",
            $"Schedule: {report.StartDateTime:u} - {report.EndDateTime:u}",
            $"Venue: {report.Venue}",
            $"Bookings: {report.TotalBookings} total / {report.ConfirmedBookings} confirmed / {report.CancelledBookings} cancelled",
            $"Tickets sold: {report.Tickets.Sum(x => x.SoldQuantity)}",
            $"Seats: {report.SeatsBooked} booked of {report.SeatCapacity}",
            $"Parking: {report.ParkingBooked} booked, {report.ParkingAvailable} available of {report.ParkingCapacity}",
            $"Ticket revenue: LKR {report.TicketRevenue:0.00}",
            $"Parking revenue: LKR {report.ParkingRevenue:0.00}",
            $"Total revenue: LKR {report.TotalRevenue:0.00}",
            $"Refunds: LKR {report.Refunds:0.00}"
        });

        // Delivery must succeed before the in-app notification is recorded.
        await emailService.SendEventReportAsync(
            report.OrganizerEmail,
            report.OrganizerName ?? "Organizer",
            report.EventName,
            summary);

        var organizerUserId = await db.Organizers
            .Where(x => x.Id == organizerId)
            .Select(x => x.UserId)
            .SingleAsync(ct);

        db.UserNotifications.Add(new UserNotification
        {
            UserId = organizerUserId,
            Title = "New Event Report",
            Message = $"Admin sent you the report for {report.EventName}.",
            Type = "Report",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
        return report;
    }

    private async Task<EventReportDto> BuildEventReportAsync(
        int eventId,
        int expectedOrganizerId,
        CancellationToken ct)
    {
        var organizer = await db.Organizers
            .AsNoTracking()
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.Id == expectedOrganizerId, ct)
            ?? throw new NotFoundException("Organizer not found.");

        var eventItem = await db.Events
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == eventId, ct)
            ?? throw new NotFoundException("Event not found.");

        if (eventItem.OrganizerId != expectedOrganizerId)
            throw new ApiException(403, "The selected event does not belong to this organizer.");

        var venue = eventItem.ExternalVenueName;
        if (string.IsNullOrWhiteSpace(venue) && eventItem.VenueId.HasValue)
        {
            venue = await db.Venues
                .AsNoTracking()
                .Where(x => x.Id == eventItem.VenueId.Value)
                .Select(x => x.Name)
                .SingleOrDefaultAsync(ct);
        }

        var bookings = await db.Bookings
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.Tickets)
            .Include(x => x.Seats)
            .Include(x => x.Parking)
            .Include(x => x.Payment)
            .Where(x => x.EventId == eventId)
            .ToListAsync(ct);

        var configuredTickets = await db.TicketTypes
            .AsNoTracking()
            .Where(x => x.EventId == eventId && x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        var paidBookings = bookings
            .Where(x => x.Status == BookingStatus.Confirmed && x.Payment?.Status == PaymentStatus.Completed)
            .ToList();

        var ticketRows = configuredTickets.Select(ticket =>
        {
            var sold = paidBookings
                .SelectMany(x => x.Tickets)
                .Where(x => string.Equals(x.TicketType, ticket.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return new EventReportTicketDto(
                ticket.Name,
                ticket.Quantity,
                sold.Sum(x => x.Quantity),
                sold.Sum(x => x.Quantity * x.UnitPrice));
        }).ToList();

        var allocations = await db.EventParkingAllocations
            .AsNoTracking()
            .Include(x => x.ParkingArea)
                .ThenInclude(x => x!.Slots)
            .Where(x => x.EventId == eventId && x.IsActive)
            .ToListAsync(ct);

        var parkingCapacity = allocations.Sum(x =>
        {
            var activeSlots = x.ParkingArea?.Slots.Count(slot => slot.IsActive) ?? 0;
            return x.AllocatedSlotCount > 0
                ? Math.Min(x.AllocatedSlotCount, activeSlots)
                : activeSlots;
        });
        var heldOrBookedParking = bookings.Count(x =>
            x.Status != BookingStatus.Cancelled && x.Parking is not null);
        var parkingBooked = paidBookings.Count(x => x.Parking is not null);
        var parkingRevenue = paidBookings.Sum(x => x.Parking?.Fee ?? 0m);
        var ticketRevenue = paidBookings.SelectMany(x => x.Tickets)
            .Sum(x => x.Quantity * x.UnitPrice);
        var payments = bookings.Where(x => x.Payment is not null).Select(x => x.Payment!).ToList();
        var totalRevenue = payments.Where(x => x.Status == PaymentStatus.Completed).Sum(x => x.Amount);
        var refunds = payments.Where(x => x.Status == PaymentStatus.Refunded).Sum(x => x.Amount);

        return new EventReportDto(
            eventItem.Id,
            eventItem.Name,
            eventItem.Status.ToString(),
            eventItem.StartDateTime,
            eventItem.EndDateTime,
            venue ?? "Venue not available",
            organizer.Id,
            organizer.OrganizationName,
            organizer.User.Email,
            bookings.Count,
            paidBookings.Count,
            bookings.Count(x => x.Status == BookingStatus.Cancelled),
            bookings.Select(x => x.CustomerId).Distinct().Count(),
            await db.Seats.CountAsync(x => x.EventId == eventId && x.IsActive, ct),
            paidBookings.Sum(x => x.Seats.Count),
            parkingCapacity,
            parkingBooked,
            Math.Max(0, parkingCapacity - heldOrBookedParking),
            paidBookings.Count(x => x.Parking is not null),
            ticketRevenue,
            parkingRevenue,
            totalRevenue,
            refunds,
            ticketRows,
            bookings.GroupBy(x => x.Status.ToString())
                .Select(x => new EventReportStatusDto(x.Key, x.Count()))
                .OrderBy(x => x.Status)
                .ToList(),
            new EventReportPaymentDto(
                payments.Count(x => x.Status == PaymentStatus.Completed),
                payments.Count(x => x.Status == PaymentStatus.PendingOtp),
                payments.Count(x => x.Status == PaymentStatus.Failed),
                payments.Count(x => x.Status == PaymentStatus.Refunded),
                totalRevenue,
                refunds),
            DateTime.UtcNow);
    }
}
