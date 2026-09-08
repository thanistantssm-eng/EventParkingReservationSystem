using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Transactions;

[ApiController, Route("api/bookings")]
public sealed class BookingsController(IBookingService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingDto request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingDto>> Get(int id, CancellationToken ct) => Ok(await service.GetAsync(id, ct));

    [HttpGet("customer/{customerId:int}")]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> CustomerHistory(int customerId, CancellationToken ct) => Ok(await service.GetCustomerBookingsAsync(customerId, ct));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> EventBookings([FromQuery] int eventId, CancellationToken ct) => Ok(await service.GetEventBookingsAsync(eventId, ct));

    [HttpGet("/api/events/{eventId:int}/availability")]
    public async Task<ActionResult<EventAvailabilityDto>> Availability(int eventId, CancellationToken ct) => Ok(await service.GetAvailabilityAsync(eventId, ct));

    [HttpPost("{id:int}/parking")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> AttachParking(
        int id,
        [FromQuery] int customerId,
        ReserveParkingDto request,
        CancellationToken ct)
    {
        var result = await service.AttachParkingAsync(id, customerId, request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpDelete("{id:int}/parking")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveParking(
        int id,
        [FromQuery] int customerId,
        CancellationToken ct)
    {
        await service.RemoveParkingAsync(id, customerId, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id, [FromQuery] int customerId, CancellationToken ct)
    { await service.CancelAsync(id, customerId, ct); return NoContent(); }
}
