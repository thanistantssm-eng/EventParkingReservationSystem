using System.Security.Claims;
using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Core;

/// <summary>
/// Resolves role profile identifiers from the authenticated user on every
/// request. The database relationship is the authorization source of truth;
/// custom JWT identifiers are transport hints and may be absent or stale.
/// </summary>
public sealed class OrganizerClaimsTransformation(
    AppDbContext db,
    ILogger<OrganizerClaimsTransformation> logger) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity ||
            !identity.IsAuthenticated)
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

        var role = principal.FindFirstValue(ClaimTypes.Role)
            ?? principal.FindFirstValue("role");

        if (string.Equals(
                role,
                UserRole.Organizer.ToString(),
                StringComparison.OrdinalIgnoreCase))
        {
            var organizerId = await db.Organizers
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.User.IsActive)
                .Select(x => (int?)x.Id)
                .SingleOrDefaultAsync();

            ReplaceProfileClaim(
                identity,
                "organizerId",
                organizerId,
                userId,
                "Organizer");
        }
        else if (string.Equals(
                     role,
                     UserRole.Customer.ToString(),
                     StringComparison.OrdinalIgnoreCase))
        {
            var customerId = await db.Customers
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.User.IsActive)
                .Select(x => (int?)x.Id)
                .SingleOrDefaultAsync();

            ReplaceProfileClaim(
                identity,
                "customerId",
                customerId,
                userId,
                "Customer");
        }

        return principal;
    }

    private void ReplaceProfileClaim(
        ClaimsIdentity identity,
        string claimType,
        int? profileId,
        int userId,
        string role)
    {

        var existingClaims = identity.Claims
            .Where(x => string.Equals(
                x.Type,
                claimType,
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var claim in existingClaims)
        {
            identity.RemoveClaim(claim);
        }

        if (profileId.HasValue)
        {
            identity.AddClaim(new Claim(
                claimType,
                profileId.Value.ToString()));

            if (existingClaims.Count == 0 ||
                existingClaims.Any(x => x.Value != profileId.Value.ToString()))
            {
                logger.LogWarning(
                    "Added or corrected {Role} profile claim for authenticated user {UserId}.",
                    role,
                    userId);
            }
        }
        else
        {
            logger.LogWarning(
                "Authenticated {Role} user {UserId} has no active role profile.",
                role,
                userId);
        }
    }
}
