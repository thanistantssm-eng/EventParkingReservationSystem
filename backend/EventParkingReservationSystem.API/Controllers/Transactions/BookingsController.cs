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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id, [FromQuery] int customerId, CancellationToken ct)
    { await service.CancelAsync(id, customerId, ct); return NoContent(); }
}
