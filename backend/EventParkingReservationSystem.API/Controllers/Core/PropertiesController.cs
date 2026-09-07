using EventParkingReservationSystem.API.DTOs.Properties;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Core;

[ApiController]
[Route("api/properties")]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;

    public PropertiesController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    // GET: api/properties
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PropertyDto>>> GetAll()
    {
        var properties = await _propertyService.GetAllAsync();

        return Ok(properties);
    }

    // GET: api/properties/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PropertyDto>> GetById(int id)
    {
        var property = await _propertyService.GetByIdAsync(id);

        if (property == null)
        {
            return NotFound(new
            {
                message = $"Property with ID {id} was not found."
            });
        }

        return Ok(property);
    }

    // POST: api/properties
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PropertyDto>> Create(
        [FromBody] CreatePropertyDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var property = await _propertyService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = property.Id },
            property);
    }

    // PUT: api/properties/5
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<PropertyDto>> Update(
        int id,
        [FromBody] UpdatePropertyDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var property = await _propertyService.UpdateAsync(id, dto);

        if (property == null)
        {
            return NotFound(new
            {
                message = $"Property with ID {id} was not found."
            });
        }

        return Ok(property);
    }

    // DELETE: api/properties/5
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _propertyService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = $"Property with ID {id} was not found."
            });
        }

        return NoContent();
    }
}