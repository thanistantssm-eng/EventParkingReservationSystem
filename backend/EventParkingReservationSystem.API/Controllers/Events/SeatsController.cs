using System.Security.Claims;
using EventParkingReservationSystem.API.DTOs.Events;
using EventParkingReservationSystem.API.Interfaces.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Events;

[ApiController]
[Route("api")]
public class SeatsController(ISeatService service) : ControllerBase
{
    private readonly ISeatService _service = service;

    [AllowAnonymous]
    [HttpGet("events/{eventId:int}/seats")]
    [ProducesResponseType(typeof(IReadOnlyList<SeatDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<SeatDto>>> GetByEvent(
        int eventId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByEventAsync(
            eventId,
            OrganizerId(),
            Role(),
            cancellationToken);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("seats/{seatId:int}")]
    [ProducesResponseType(typeof(SeatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeatDto>> GetById(
        int seatId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(
            seatId,
            OrganizerId(),
            Role(),
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Roles = "Admin,Organizer")]
    [HttpPost("events/{eventId:int}/seats")]
    [ProducesResponseType(typeof(SeatDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SeatDto>> Create(
        int eventId,
        CreateSeatDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(
            eventId,
            dto,
            OrganizerId(),
            Role(),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { seatId = created.Id },
            created);
    }

    [Authorize(Roles = "Admin,Organizer")]
    [HttpPost("events/{eventId:int}/seats/bulk")]
    [ProducesResponseType(typeof(IReadOnlyList<SeatDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IReadOnlyList<SeatDto>>> CreateBulk(
        int eventId,
        List<CreateSeatDto> dtos,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateBulkAsync(
            eventId,
            dtos,
            OrganizerId(),
            Role(),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetByEvent),
            new { eventId },
            created);
    }

    [Authorize(Roles = "Admin,Organizer")]
    [HttpPut("seats/{seatId:int}")]
    [ProducesResponseType(typeof(SeatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SeatDto>> Update(
        int seatId,
        UpdateSeatDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAsync(
            seatId,
            dto,
            OrganizerId(),
            Role(),
            cancellationToken);

        return Ok(updated);
    }

    [Authorize(Roles = "Admin,Organizer")]
    [HttpDelete("seats/{seatId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        int seatId,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(
            seatId,
            OrganizerId(),
            Role(),
            cancellationToken);

        return NoContent();
    }

    private int? OrganizerId()
    {
        var value =
            User.FindFirstValue("organizerId") ??
            User.FindFirstValue("OrganizerId");

        return int.TryParse(value, out var id) ? id : null;
    }

    private string Role() =>
        User.FindFirstValue(ClaimTypes.Role) ??
        User.FindFirstValue("role") ??
        string.Empty;
}
