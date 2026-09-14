using System.Security.Claims;
using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Core;

/// <summary>
/// Resolves the organizer profile from the authenticated user on every request.
/// The database relationship is the authorization source of truth; the JWT
/// organizerId claim is only a transport hint and may be stale in an old token.
/// </summary>
public sealed class OrganizerClaimsTransformation(
    AppDbContext db,
    ILogger<OrganizerClaimsTransformation> logger) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity ||
            !identity.IsAuthenticated ||
            !IsOrganizer(principal))
        {
            return principal;
        }

        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub")
            ?? principal.FindFirstValue("userId");

        if (!int.TryParse(userIdValue, out var userId))
        {
            return principal;
        }

        var organizerId = await db.Organizers
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.User.IsActive)
            .Select(x => (int?)x.Id)
            .SingleOrDefaultAsync();

        var existingClaims = identity.Claims
            .Where(x => string.Equals(
                x.Type,
                "organizerId",
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var claim in existingClaims)
        {
            identity.RemoveClaim(claim);
        }

        if (organizerId.HasValue)
        {
            identity.AddClaim(new Claim(
                "organizerId",
                organizerId.Value.ToString()));

            if (existingClaims.Any(x => x.Value != organizerId.Value.ToString()))
            {
                logger.LogWarning(
                    "Corrected stale organizer identity claim for authenticated user {UserId}.",
                    userId);
            }
        }
        else
        {
            logger.LogWarning(
                "Authenticated Organizer user {UserId} has no active organizer profile.",
                userId);
        }

        return principal;
    }

    private static bool IsOrganizer(ClaimsPrincipal principal)
    {
        var role = principal.FindFirstValue(ClaimTypes.Role)
            ?? principal.FindFirstValue("role");

        return string.Equals(
            role,
            UserRole.Organizer.ToString(),
            StringComparison.OrdinalIgnoreCase);
    }
}
