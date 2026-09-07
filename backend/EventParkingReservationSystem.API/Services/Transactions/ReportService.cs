using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Models.Transactions;
using Microsoft.EntityFrameworkCore;

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
}
