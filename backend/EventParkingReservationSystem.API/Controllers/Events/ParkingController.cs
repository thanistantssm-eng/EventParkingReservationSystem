using System.Security.Claims;
using EventParkingReservationSystem.API.DTOs.Events;
using EventParkingReservationSystem.API.Interfaces.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Events;

[ApiController]
[Route("api/parking")]
public class ParkingController(IParkingService service) : ControllerBase
{
    private readonly IParkingService _service = service;

    [AllowAnonymous]
    [HttpGet("areas")]
    [ProducesResponseType(typeof(IReadOnlyList<ParkingAreaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ParkingAreaDto>>> GetAreas(
        [FromQuery] int? venueId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAreasAsync(
            venueId,
            cancellationToken);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("areas/{areaId:int}")]
    [ProducesResponseType(typeof(ParkingAreaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParkingAreaDto>> GetAreaById(
        int areaId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAreaByIdAsync(
            areaId,
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("areas")]
    [ProducesResponseType(typeof(ParkingAreaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ParkingAreaDto>> CreateArea(
        CreateParkingAreaDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateAreaAsync(
            dto,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetAreaById),
            new { areaId = created.Id },
            created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("areas/{areaId:int}")]
    [ProducesResponseType(typeof(ParkingAreaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ParkingAreaDto>> UpdateArea(
        int areaId,
        UpdateParkingAreaDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAreaAsync(
            areaId,
            dto,
            cancellationToken);

        return Ok(updated);
    }

    [AllowAnonymous]
    [HttpGet("areas/{areaId:int}/slots")]
    [ProducesResponseType(typeof(IReadOnlyList<ParkingSlotDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ParkingSlotDto>>> GetSlots(
        int areaId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetSlotsAsync(
            areaId,
            cancellationToken);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("slots/{slotId:int}")]
    [ProducesResponseType(typeof(ParkingSlotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParkingSlotDto>> GetSlotById(
        int slotId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetSlotByIdAsync(
            slotId,
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("areas/{areaId:int}/slots")]
    [ProducesResponseType(typeof(ParkingSlotDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ParkingSlotDto>> CreateSlot(
        int areaId,
        CreateParkingSlotDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateSlotAsync(
            areaId,
            dto,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetSlotById),
            new { slotId = created.Id },
            created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("slots/{slotId:int}")]
    [ProducesResponseType(typeof(ParkingSlotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ParkingSlotDto>> UpdateSlot(
        int slotId,
        UpdateParkingSlotDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateSlotAsync(
            slotId,
            dto,
            cancellationToken);

        return Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("slots/{slotId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteSlot(
        int slotId,
        CancellationToken cancellationToken)
    {
        await _service.DeleteSlotAsync(
            slotId,
            cancellationToken);

        return NoContent();
    }

    // Enhanced layout endpoint retained for the admin/organizer UI.
    [AllowAnonymous]
    [HttpGet("events/{eventId:int}/layout")]
    [ProducesResponseType(typeof(ParkingLayoutDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParkingLayoutDto>> GetEventLayout(
        int eventId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetEventLayoutAsync(
            eventId,
            OrganizerId(),
            Role(),
            cancellationToken);

        return Ok(result);
    }

    // Exact BRD endpoint consumed by Angular:
    // GET /api/events/{eventId}/parking-slots
    [AllowAnonymous]
    [HttpGet("~/api/events/{eventId:int}/parking-slots")]
    [ProducesResponseType(typeof(IReadOnlyList<ParkingSlotDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ParkingSlotDto>>> GetEventParkingSlots(
        int eventId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetEventParkingSlotsAsync(
            eventId,
            OrganizerId(),
            Role(),
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Roles = "Admin,Organizer")]
    [HttpPost("events/{eventId:int}/allocations")]
    [ProducesResponseType(typeof(EventParkingAllocationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EventParkingAllocationDto>> AllocateArea(
        int eventId,
        CreateEventParkingAllocationDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _service.AllocateAreaAsync(
            eventId,
            dto,
            OrganizerId(),
            Role(),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetEventLayout),
            new { eventId },
            created);
    }

    [Authorize(Roles = "Admin,Organizer")]
    [HttpDelete("allocations/{allocationId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAllocation(
        int allocationId,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAllocationAsync(
            allocationId,
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
