using EventParkingReservationSystem.API.DTOs.Venues;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Core;

[ApiController]
[Route("api/venues")]
public class VenuesController : ControllerBase
{
    private readonly IVenueService _service;

    public VenuesController(
        IVenueService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> All(
        [FromQuery] int? propertyId)
    {
        return Ok(
            await _service
                .GetAllAsync(propertyId));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(
        int id)
    {
        var result =
            await _service.GetByIdAsync(id);

        return result == null
            ? NotFound()
            : Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateVenueDto request)
    {
        try
        {
            var result =
                await _service.CreateAsync(
                    request);

            return CreatedAtAction(
                nameof(Get),
                new { id = result.Id },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateVenueDto request)
    {
        try
        {
            var result =
                await _service.UpdateAsync(
                    id,
                    request);

            return result == null
                ? NotFound()
                : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id)
    {
        var deleted =
            await _service.DeleteAsync(id);

        return deleted
            ? NoContent()
            : NotFound();
    }
}