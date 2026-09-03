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
    public async Task<ActionResult<IReadOnlyList<TicketTypeDto>>> GetByEvent(
        int eventId,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetByEventAsync(
            eventId,
            OrganizerId(),
            Role(),
            cancellationToken));

    [Authorize(Roles = "Admin,Organizer")]
    [HttpPost("events/{eventId:int}/tickets")]
    public async Task<ActionResult<TicketTypeDto>> Create(
        int eventId,
        CreateTicketTypeDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _service.CreateAsync(
            eventId,
            dto,
            OrganizerId(),
            Role(),
            cancellationToken));

    [Authorize(Roles = "Admin,Organizer")]
    [HttpPut("tickets/{ticketTypeId:int}")]
    public async Task<ActionResult<TicketTypeDto>> Update(
        int ticketTypeId,
        UpdateTicketTypeDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _service.UpdateAsync(
            ticketTypeId,
            dto,
            OrganizerId(),
            Role(),
            cancellationToken));

    [Authorize(Roles = "Admin,Organizer")]
    [HttpDelete("tickets/{ticketTypeId:int}")]
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
