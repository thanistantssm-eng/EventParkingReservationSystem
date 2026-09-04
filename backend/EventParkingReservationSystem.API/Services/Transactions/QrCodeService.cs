using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Middleware;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Transactions;

public sealed class QrCodeService(AppDbContext db) : IQrCodeService
{
    public async Task<QrCodeDto> GetForBookingAsync(int bookingId, CancellationToken ct) => Map(
        await db.QrCodes.AsNoTracking().SingleOrDefaultAsync(x => x.BookingId == bookingId, ct) ?? throw new NotFoundException("Booking QR not found."));

    public async Task<QrCodeDto> ValidateAsync(string token, CancellationToken ct) => Map(
        await db.QrCodes.AsNoTracking().SingleOrDefaultAsync(x => x.Token == token, ct) ?? throw new NotFoundException("QR token is invalid."));

    private static QrCodeDto Map(Models.Transactions.QrCode x) => new(x.BookingId, x.Token, x.Payload, x.CreatedAtUtc);
}
