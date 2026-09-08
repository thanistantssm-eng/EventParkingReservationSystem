using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Models.Transactions;
using Microsoft.EntityFrameworkCore;
using EventParkingReservationSystem.API.Middleware;

namespace EventParkingReservationSystem.API.Services.Transactions;

public sealed class ReportService(AppDbContext db) : IReportService
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
}
