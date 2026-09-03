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
    public async Task<ActionResult<IReadOnlyList<SeatDto>>> GetByEvent(
        int eventId,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetByEventAsync(
            eventId,
            OrganizerId(),
            Role(),
            cancellationToken));

    [Authorize(Roles = "Admin,Organizer")]
    [HttpPost("events/{eventId:int}/seats")]
    public async Task<ActionResult<SeatDto>> Create(
        int eventId,
        CreateSeatDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _service.CreateAsync(
            eventId,
            dto,
            OrganizerId(),
            Role(),
            cancellationToken));

    [Authorize(Roles = "Admin,Organizer")]
    [HttpPost("events/{eventId:int}/seats/bulk")]
    public async Task<ActionResult<IReadOnlyList<SeatDto>>> CreateBulk(
        int eventId,
        List<CreateSeatDto> dtos,
        CancellationToken cancellationToken) =>
        Ok(await _service.CreateBulkAsync(
            eventId,
            dtos,
            OrganizerId(),
            Role(),
            cancellationToken));

    [Authorize(Roles = "Admin,Organizer")]
    [HttpPut("seats/{seatId:int}")]
    public async Task<ActionResult<SeatDto>> Update(
        int seatId,
        UpdateSeatDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _service.UpdateAsync(
            seatId,
            dto,
            OrganizerId(),
            Role(),
            cancellationToken));

    [Authorize(Roles = "Admin,Organizer")]
    [HttpDelete("seats/{seatId:int}")]
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
