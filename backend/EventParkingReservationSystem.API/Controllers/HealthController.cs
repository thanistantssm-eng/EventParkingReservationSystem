using EventParkingReservationSystem.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController(
    AppDbContext db,
    ILogger<HealthController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        try
        {
            if (!await db.Database.CanConnectAsync(
                    cancellationToken))
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new
                    {
                        status = "unavailable",
                        message = "The API is running, but the database is unavailable."
                    });
            }

            return Ok(new
            {
                status = "ok",
                message = "Event & Parking Reservation System backend and database are running."
            });
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Database health check failed.");

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    status = "unavailable",
                    message = "The API is running, but the database is unavailable."
                });
        }
    }
}
