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
[Route("api/bookings")]
[Authorize]
public sealed class BookingsController(
    IBookingService service,
    AppDbContext db) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<BookingDto>> Create(
        CreateBookingDto request,
        CancellationToken ct)
    {
        // Never trust CustomerId supplied by the client.
        request.CustomerId = User.RequireCustomerId();

        var result = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Organizer,Customer")]
    public async Task<ActionResult<BookingDto>> Get(
        int id,
        CancellationToken ct)
    {
        await EnsureBookingAccessAsync(id, ct);
        return Ok(await service.GetAsync(id, ct));
    }

    [HttpGet("me")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> MyHistory(
        CancellationToken ct) =>
        Ok(await service.GetCustomerBookingsAsync(
            User.RequireCustomerId(),
            ct));

    [HttpGet("customer/{customerId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> CustomerHistory(
        int customerId,
        CancellationToken ct) =>
        Ok(await service.GetCustomerBookingsAsync(customerId, ct));

    [HttpGet]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> EventBookings(
        [FromQuery] int eventId,
        CancellationToken ct)
    {
        if (User.RequireRole() == "Organizer")
        {
            var organizerId = User.RequireOrganizerId();
            var ownsEvent = await db.Events
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == eventId && x.OrganizerId == organizerId,
                    ct);

            if (!ownsEvent)
            {
                throw new ApiException(
                    403,
                    "You can only view bookings for your own events.");
            }
        }

        return Ok(await service.GetEventBookingsAsync(eventId, ct));
    }

    [AllowAnonymous]
    [HttpGet("/api/events/{eventId:int}/availability")]
    public async Task<ActionResult<EventAvailabilityDto>> Availability(
        int eventId,
        CancellationToken ct) =>
        Ok(await service.GetAvailabilityAsync(eventId, ct));

    [HttpPost("{id:int}/parking")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> AttachParking(
        int id,
        ReserveParkingDto request,
        CancellationToken ct)
    {
        var result = await service.AttachParkingAsync(
            id,
            User.RequireCustomerId(),
            request,
            ct);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpDelete("{id:int}/parking")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveParking(
        int id,
        CancellationToken ct)
    {
        await service.RemoveParkingAsync(
            id,
            User.RequireCustomerId(),
            ct);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Cancel(
        int id,
        CancellationToken ct)
    {
        await service.CancelAsync(
            id,
            User.RequireCustomerId(),
            ct);

        return NoContent();
    }

    private async Task EnsureBookingAccessAsync(
        int bookingId,
        CancellationToken ct)
    {
        var role = User.RequireRole();

        if (role == "Admin")
        {
            return;
        }

        bool allowed;

        if (role == "Customer")
        {
            var customerId = User.RequireCustomerId();
            allowed = await db.Bookings
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == bookingId && x.CustomerId == customerId,
                    ct);
        }
        else if (role == "Organizer")
        {
            var organizerId = User.RequireOrganizerId();
            allowed = await db.Bookings
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == bookingId &&
                         x.Event.OrganizerId == organizerId,
                    ct);
        }
        else
        {
            allowed = false;
        }

        if (!allowed)
        {
            throw new ApiException(403, "You do not have access to this booking.");
        }
    }
}
