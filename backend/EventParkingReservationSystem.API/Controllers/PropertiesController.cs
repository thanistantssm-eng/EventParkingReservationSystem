using EventParkingReservationSystem.API.DTOs.Properties;
using EventParkingReservationSystem.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers;

[ApiController]
[Route("api/properties")]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _service;

    public PropertiesController(
        IPropertyService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(
            await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<IActionResult> GetById(int id)
    {
        return Ok(
            await _service.GetByIdAsync(id));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        CreatePropertyDto request)
    {
        var result =
            await _service.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        UpdatePropertyDto request)
    {
        return Ok(
            await _service.UpdateAsync(
                id,
                request));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeactivateAsync(id);

        return NoContent();
    }
}