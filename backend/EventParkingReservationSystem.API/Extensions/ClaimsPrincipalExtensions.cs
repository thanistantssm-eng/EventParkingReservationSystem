using System.Security.Claims;

namespace EventParkingReservationSystem.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int RequireUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub")
            ?? user.FindFirstValue("userId");

        return int.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("User id claim is missing.");
    }

    public static int RequireCustomerId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("customerId")
            ?? user.FindFirstValue("CustomerId");

        return int.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Customer id claim is missing.");
    }

    public static int RequireOrganizerId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("organizerId")
            ?? user.FindFirstValue("OrganizerId");

        return int.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Organizer id claim is missing.");
    }

    public static string RequireRole(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Role)
        ?? user.FindFirstValue("role")
        ?? throw new UnauthorizedAccessException("Role claim is missing.");
}
