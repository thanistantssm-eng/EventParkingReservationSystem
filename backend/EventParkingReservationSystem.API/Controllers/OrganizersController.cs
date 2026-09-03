using EventParkingReservationSystem.API.DTOs.Organizers;
using EventParkingReservationSystem.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers;

[ApiController]
[Route("api/organizers")]
[Authorize(Roles = "Admin")]
public class OrganizersController : ControllerBase
{
    private readonly IOrganizerService _service;

    public OrganizersController(
        IOrganizerService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(
            await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        return Ok(
            await _service.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateOrganizerDto request)
    {
        var result =
            await _service.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateOrganizerDto request)
    {
        return Ok(
            await _service.UpdateAsync(
                id,
                request));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeactivateAsync(id);

        return NoContent();
    }
}