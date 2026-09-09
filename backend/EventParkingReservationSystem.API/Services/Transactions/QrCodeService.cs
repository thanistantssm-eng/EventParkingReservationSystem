using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Middleware;
using EventParkingReservationSystem.API.Models.Transactions;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Transactions;

public sealed class QrCodeService(AppDbContext db) : IQrCodeService
{
    public async Task<QrCodeDto> GetForBookingAsync(
        int bookingId,
        CancellationToken ct)
    {
        var qr = await db.QrCodes
            .AsNoTracking()
            .Include(x => x.Booking)
                .ThenInclude(x => x.Payment)
            .SingleOrDefaultAsync(x => x.BookingId == bookingId, ct)
            ?? throw new NotFoundException("Booking QR not found.");

        EnsureQrIsActive(qr);
        return Map(qr);
    }

    public async Task<QrCodeDto> ValidateAsync(
        string token,
        CancellationToken ct)
    {
        var qr = await db.QrCodes
            .AsNoTracking()
            .Include(x => x.Booking)
                .ThenInclude(x => x.Payment)
            .SingleOrDefaultAsync(x => x.Token == token, ct)
            ?? throw new NotFoundException("QR token is invalid.");

        EnsureQrIsActive(qr);
        return Map(qr);
    }

    private static void EnsureQrIsActive(Models.Transactions.QrCode qr)
    {
        if (qr.Booking.Status != BookingStatus.Confirmed ||
            qr.Booking.Payment?.Status != PaymentStatus.Completed)
        {
            throw new ConflictException(
                "This booking QR is no longer valid.");
        }
    }

    private static QrCodeDto Map(Models.Transactions.QrCode x) =>
        new(x.BookingId, x.Token, x.Payload, x.CreatedAtUtc);
}
