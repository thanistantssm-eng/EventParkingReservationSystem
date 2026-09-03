using System.Security.Claims;
using EventParkingReservationSystem.API.DTOs.Events;
using EventParkingReservationSystem.API.Interfaces.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Events;

[ApiController]
[Route("api/events")]
public class EventsController(IEventService service) : ControllerBase
{
    private readonly IEventService _service = service;

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetAll(
        [FromQuery] EventQueryDto query,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetAllAsync(
            query,
            TryUserId(),
            OrganizerId(),
            Role(),
            cancellationToken));

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetById(
        int id,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetByIdAsync(
            id,
            TryUserId(),
            OrganizerId(),
            Role(),
            cancellationToken));

    [AllowAnonymous]
    [HttpGet("qr/{qrCode}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetByQrCode(
        string qrCode,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetByQrCodeAsync(
            qrCode,
            OrganizerId(),
            Role(),
            cancellationToken));

    [Authorize(Roles = "Admin,Organizer")]
    [HttpPost]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EventDto>> Create(
        CreateEventDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(
            dto,
            UserId(),
            OrganizerId(),
            Role(),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/for-organizer/{organizerId:int}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<EventDto>> CreateForOrganizer(
        int organizerId,
        CreateEventDto dto,
        CancellationToken cancellationToken)
    {
        dto.OrganizerId = organizerId;

        var created = await _service.CreateAsync(
            dto,
            UserId(),
            null,
            "Admin",
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    [Authorize(Roles = "Admin,Organizer")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> Update(
        int id,
        UpdateEventDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _service.UpdateAsync(
            id,
            dto,
            UserId(),
            OrganizerId(),
            Role(),
            cancellationToken));

    [Authorize(Roles = "Admin,Organizer")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(
            id,
            UserId(),
            OrganizerId(),
            Role(),
            cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/publish")]
    public async Task<ActionResult<EventDto>> Publish(
        int id,
        CancellationToken cancellationToken) =>
        Ok(await _service.PublishAsync(
            id,
            UserId(),
            Role(),
            cancellationToken));

    [Authorize(Roles = "Admin,Organizer")]
    [HttpPost("{id:int}/qr/regenerate")]
    public async Task<ActionResult<EventDto>> RegenerateQr(
        int id,
        CancellationToken cancellationToken) =>
        Ok(await _service.RegenerateQrAsync(
            id,
            UserId(),
            OrganizerId(),
            Role(),
            cancellationToken));

    private int? TryUserId()
    {
        var value =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub") ??
            User.FindFirstValue("userId");

        return int.TryParse(value, out var id) ? id : null;
    }

    private int UserId()
    {
        var value =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub") ??
            User.FindFirstValue("userId");

        return int.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException(
                "User id claim is missing.");
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
