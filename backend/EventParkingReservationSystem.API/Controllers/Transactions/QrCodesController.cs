using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Extensions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Controllers.Transactions;

[ApiController]
[Route("api/qr-codes")]
[Authorize]
public sealed class QrCodesController(
    IQrCodeService service,
    AppDbContext db) : ControllerBase
{
    [HttpGet("booking/{bookingId:int}")]
    [Authorize(Roles = "Admin,Organizer,Customer")]
    public async Task<ActionResult<QrCodeDto>> BookingQr(
        int bookingId,
        CancellationToken ct)
    {
        await EnsureBookingAccessAsync(bookingId, ct);
        return Ok(await service.GetForBookingAsync(bookingId, ct));
    }

    [HttpGet("validate/{token}")]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<ActionResult<QrCodeDto>> Validate(
        string token,
        CancellationToken ct)
    {
        if (User.RequireRole() == "Organizer")
        {
            var organizerId = User.RequireOrganizerId();
            var ownsQr = await db.QrCodes
                .AsNoTracking()
                .AnyAsync(
                    x => x.Token == token &&
                         x.Booking.Event.OrganizerId == organizerId,
                    ct);

            if (!ownsQr)
            {
                throw new ApiException(
                    403,
                    "You can only validate booking QR codes for your own events.");
            }
        }

        return Ok(await service.ValidateAsync(token, ct));
    }

    private async Task EnsureBookingAccessAsync(
        int bookingId,
        CancellationToken ct)
    {
        var role = User.RequireRole();
        if (role == "Admin") return;

        var allowed = role switch
        {
            "Customer" => await db.Bookings
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == bookingId &&
                         x.CustomerId == User.RequireCustomerId(),
                    ct),

            "Organizer" => await db.Bookings
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == bookingId &&
                         x.Event.OrganizerId == User.RequireOrganizerId(),
                    ct),

            _ => false
        };

        if (!allowed)
        {
            throw new ApiException(403, "You do not have access to this booking QR.");
        }
    }
}
