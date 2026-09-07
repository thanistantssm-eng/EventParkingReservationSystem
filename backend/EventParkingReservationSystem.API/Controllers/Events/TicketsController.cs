using System.Security.Claims;
using EventParkingReservationSystem.API.DTOs.Events;
using EventParkingReservationSystem.API.Interfaces.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Events;

[ApiController]
[Route("api")]
public class TicketsController(ITicketService service) : ControllerBase
{
    private readonly ITicketService _service = service;

    [AllowAnonymous]
    [HttpGet("events/{eventId:int}/tickets")]
    [ProducesResponseType(typeof(IReadOnlyList<TicketTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TicketTypeDto>>> GetByEvent(
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
    [HttpGet("tickets/{ticketTypeId:int}")]
    [ProducesResponseType(typeof(TicketTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketTypeDto>> GetById(
        int ticketTypeId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(
            ticketTypeId,
            OrganizerId(),
            Role(),
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Roles = "Admin,Organizer")]
    [HttpPost("events/{eventId:int}/tickets")]
    [ProducesResponseType(typeof(TicketTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketTypeDto>> Create(
        int eventId,
        CreateTicketTypeDto dto,
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
            new { ticketTypeId = created.Id },
            created);
    }

    [Authorize(Roles = "Admin,Organizer")]
    [HttpPut("tickets/{ticketTypeId:int}")]
    [ProducesResponseType(typeof(TicketTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketTypeDto>> Update(
        int ticketTypeId,
        UpdateTicketTypeDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAsync(
            ticketTypeId,
            dto,
            OrganizerId(),
            Role(),
            cancellationToken);

        return Ok(updated);
    }

    [Authorize(Roles = "Admin,Organizer")]
    [HttpDelete("tickets/{ticketTypeId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        int ticketTypeId,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(
            ticketTypeId,
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
