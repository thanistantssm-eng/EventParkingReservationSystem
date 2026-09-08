using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Dashboards;
using EventParkingReservationSystem.API.Interfaces.Services.Dashboards;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Models.Transactions;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Dashboards;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardDto> GetAdminAsync(
        CancellationToken cancellationToken = default)
    {
        var revenue = await _context.Payments
            .Where(x => x.Status == PaymentStatus.Completed)
            .SumAsync(
                x => (decimal?)x.Amount,
                cancellationToken) ?? 0m;

        return new AdminDashboardDto
        {
            TotalUsers = await _context.Users
                .CountAsync(cancellationToken),

            ActiveUsers = await _context.Users
                .CountAsync(x => x.IsActive, cancellationToken),

            TotalCustomers = await _context.Customers
                .CountAsync(cancellationToken),

            TotalOrganizers = await _context.Organizers
                .CountAsync(cancellationToken),

            PendingOrganizerVerifications = await _context.Organizers
                .CountAsync(x => !x.IsVerified, cancellationToken),

            TotalProperties = await _context.Properties
                .CountAsync(cancellationToken),

            TotalVenues = await _context.Venues
                .CountAsync(cancellationToken),

            TotalEvents = await _context.Events
                .CountAsync(cancellationToken),

            PublishedEvents = await _context.Events
                .CountAsync(
                    x => x.Status == EventStatus.Published,
                    cancellationToken),

            PendingApprovalEvents = await _context.Events
                .CountAsync(
                    x => x.Status == EventStatus.PendingApproval,
                    cancellationToken),

            TotalBookings = await _context.Bookings
                .CountAsync(cancellationToken),

            ConfirmedBookings = await _context.Bookings
                .CountAsync(
                    x => x.Status == BookingStatus.Confirmed,
                    cancellationToken),

            CompletedPayments = await _context.Payments
                .CountAsync(
                    x => x.Status == PaymentStatus.Completed,
                    cancellationToken),

            TotalRevenue = revenue,

            ReservedSeats = await _context.BookingSeats
                .CountAsync(cancellationToken),

            ReservedParkingSlots = await _context.BookingParkings
                .CountAsync(cancellationToken)
        };
    }

    public async Task<OrganizerDashboardDto> GetOrganizerAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var organizer = await _context.Organizers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Organizer profile not found.");

        var events = _context.Events
            .AsNoTracking()
            .Where(x => x.OrganizerId == organizer.Id);

        var bookings = _context.Bookings
            .AsNoTracking()
            .Where(x => x.Event.OrganizerId == organizer.Id);

        var revenue = await _context.Payments
            .AsNoTracking()
            .Where(x =>
                x.Status == PaymentStatus.Completed &&
                x.Booking.Event.OrganizerId == organizer.Id)
            .SumAsync(
                x => (decimal?)x.Amount,
                cancellationToken) ?? 0m;

        return new OrganizerDashboardDto
        {
            OrganizerId = organizer.Id,
            OrganizationName = organizer.OrganizationName,
            IsVerified = organizer.IsVerified,

            TotalEvents = await events
                .CountAsync(cancellationToken),

            DraftEvents = await events
                .CountAsync(
                    x => x.Status == EventStatus.Draft,
                    cancellationToken),

            PendingApprovalEvents = await events
                .CountAsync(
                    x => x.Status == EventStatus.PendingApproval,
                    cancellationToken),

            PublishedEvents = await events
                .CountAsync(
                    x => x.Status == EventStatus.Published,
                    cancellationToken),

            UpcomingEvents = await events
                .CountAsync(
                    x => x.StartDateTime > DateTime.UtcNow &&
                         x.Status == EventStatus.Published,
                    cancellationToken),

            TotalBookings = await bookings
                .CountAsync(cancellationToken),

            ConfirmedBookings = await bookings
                .CountAsync(
                    x => x.Status == BookingStatus.Confirmed,
                    cancellationToken),

            TotalRevenue = revenue
        };
    }

    public async Task<CustomerDashboardDto> GetCustomerAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Customer profile not found.");

        var bookings = _context.Bookings
            .AsNoTracking()
            .Where(x => x.CustomerId == customer.Id);

        var spent = await _context.Payments
            .AsNoTracking()
            .Where(x =>
                x.Booking.CustomerId == customer.Id &&
                x.Status == PaymentStatus.Completed)
            .SumAsync(
                x => (decimal?)x.Amount,
                cancellationToken) ?? 0m;

        return new CustomerDashboardDto
        {
            CustomerId = customer.Id,
            Name = customer.Name,

            TotalBookings = await bookings
                .CountAsync(cancellationToken),

            PendingBookings = await bookings
                .CountAsync(
                    x => x.Status == BookingStatus.PendingPayment,
                    cancellationToken),

            ConfirmedBookings = await bookings
                .CountAsync(
                    x => x.Status == BookingStatus.Confirmed,
                    cancellationToken),

            CancelledBookings = await bookings
                .CountAsync(
                    x => x.Status == BookingStatus.Cancelled,
                    cancellationToken),

            UpcomingBookings = await bookings
                .CountAsync(
                    x => x.Status == BookingStatus.Confirmed &&
                         x.Event.StartDateTime > DateTime.UtcNow,
                    cancellationToken),

            TotalSpent = spent
        };
    }
}