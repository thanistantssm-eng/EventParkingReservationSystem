using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Events;
using EventParkingReservationSystem.API.Extensions;
using EventParkingReservationSystem.API.Middleware;
using EventParkingReservationSystem.API.Models.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Controllers.Events;

[ApiController]
[Route("api/favorites")]
[Authorize(Roles = "Customer")]
public class FavoritesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FavoriteEventDto>>> GetMine(CancellationToken ct)
    {
        var customerId = User.RequireCustomerId();
        var favorites = await db.EventFavorites
            .AsNoTracking()
            .Include(x => x.Event)
                .ThenInclude(x => x.EventCategory)
            .Where(x => x.CustomerId == customerId && x.Event.Status == EventStatus.Published)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);

        return Ok(favorites.Select(Map).ToList());
    }

    [HttpPost("/api/events/{eventId:int}/favorite")]
    public async Task<ActionResult<FavoriteEventDto>> Add(int eventId, CancellationToken ct)
    {
        var customerId = User.RequireCustomerId();
        var evt = await db.Events
            .Include(x => x.EventCategory)
            .SingleOrDefaultAsync(x => x.Id == eventId && x.Status == EventStatus.Published, ct)
            ?? throw new NotFoundException("Published event not found.");

        var existing = await db.EventFavorites
            .Include(x => x.Event)
                .ThenInclude(x => x.EventCategory)
            .SingleOrDefaultAsync(x => x.CustomerId == customerId && x.EventId == eventId, ct);

        if (existing is not null) return Ok(Map(existing));

        var favorite = new EventFavorite { CustomerId = customerId, EventId = eventId };
        db.EventFavorites.Add(favorite);
        await db.SaveChangesAsync(ct);
        favorite.Event = evt;
        return Created($"/api/events/{eventId}/favorite", Map(favorite));
    }

    [HttpDelete("/api/events/{eventId:int}/favorite")]
    public async Task<IActionResult> Remove(int eventId, CancellationToken ct)
    {
        var customerId = User.RequireCustomerId();
        var favorite = await db.EventFavorites
            .SingleOrDefaultAsync(x => x.CustomerId == customerId && x.EventId == eventId, ct)
            ?? throw new NotFoundException("Favorite event not found.");
        db.EventFavorites.Remove(favorite);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static FavoriteEventDto Map(EventFavorite x) => new(
        x.Id,
        x.EventId,
        x.Event.Name,
        x.Event.PosterUrl,
        x.Event.EventCategory?.Name,
        x.Event.StartDateTime,
        x.Event.TicketPrice,
        x.CreatedAtUtc);
}
