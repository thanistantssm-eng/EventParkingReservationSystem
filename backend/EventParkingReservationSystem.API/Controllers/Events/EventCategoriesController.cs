using EventParkingReservationSystem.API.DTOs.Events;
using EventParkingReservationSystem.API.Interfaces.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Events;

[ApiController]
[Route("api/categories")]
[Route("api/event-categories")]
public class EventCategoriesController(IEventCategoryService service) : ControllerBase
{
    private readonly IEventCategoryService _service = service;

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventCategoryDto>>> GetAll([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default) =>
        Ok(await _service.GetAllAsync(includeInactive && User.IsInRole("Admin"), cancellationToken));

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventCategoryDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _service.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<EventCategoryDto>> Create(CreateEventCategoryDto dto, CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<EventCategoryDto>> Update(int id, UpdateEventCategoryDto dto, CancellationToken cancellationToken) =>
        Ok(await _service.UpdateAsync(id, dto, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
