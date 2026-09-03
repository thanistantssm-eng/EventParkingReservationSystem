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
    public async Task<ActionResult<IReadOnlyList<ParkingAreaDto>>> GetAreas(
        [FromQuery] int? venueId,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetAreasAsync(
            venueId,
            cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("areas")]
    public async Task<ActionResult<ParkingAreaDto>> CreateArea(
        CreateParkingAreaDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _service.CreateAreaAsync(
            dto,
            cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPut("areas/{areaId:int}")]
    public async Task<ActionResult<ParkingAreaDto>> UpdateArea(
        int areaId,
        UpdateParkingAreaDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _service.UpdateAreaAsync(
            areaId,
            dto,
            cancellationToken));

    [AllowAnonymous]
    [HttpGet("areas/{areaId:int}/slots")]
    public async Task<ActionResult<IReadOnlyList<ParkingSlotDto>>> GetSlots(
        int areaId,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetSlotsAsync(
            areaId,
            cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("areas/{areaId:int}/slots")]
    public async Task<ActionResult<ParkingSlotDto>> CreateSlot(
        int areaId,
        CreateParkingSlotDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _service.CreateSlotAsync(
            areaId,
            dto,
            cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPut("slots/{slotId:int}")]
    public async Task<ActionResult<ParkingSlotDto>> UpdateSlot(
        int slotId,
        UpdateParkingSlotDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _service.UpdateSlotAsync(
            slotId,
            dto,
            cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpDelete("slots/{slotId:int}")]
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
    public async Task<ActionResult<ParkingLayoutDto>> GetEventLayout(
        int eventId,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetEventLayoutAsync(
            eventId,
            OrganizerId(),
            Role(),
            cancellationToken));

    // Exact BRD endpoint consumed by Angular:
    // GET /api/events/{eventId}/parking-slots
    [AllowAnonymous]
    [HttpGet("~/api/events/{eventId:int}/parking-slots")]
    [ProducesResponseType(typeof(IReadOnlyList<ParkingSlotDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ParkingSlotDto>>> GetEventParkingSlots(
        int eventId,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetEventParkingSlotsAsync(
            eventId,
            OrganizerId(),
            Role(),
            cancellationToken));

    [Authorize(Roles = "Admin,Organizer")]
    [HttpPost("events/{eventId:int}/allocations")]
    public async Task<ActionResult<EventParkingAllocationDto>> AllocateArea(
        int eventId,
        CreateEventParkingAllocationDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _service.AllocateAreaAsync(
            eventId,
            dto,
            OrganizerId(),
            Role(),
            cancellationToken));

    [Authorize(Roles = "Admin,Organizer")]
    [HttpDelete("allocations/{allocationId:int}")]
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
