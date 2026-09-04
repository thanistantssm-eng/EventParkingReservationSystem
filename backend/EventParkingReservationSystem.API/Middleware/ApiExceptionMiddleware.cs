using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Middleware;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (ApiException ex) { await WriteProblem(context, ex.StatusCode, ex.Message); }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Database constraint rejected the request");
            await WriteProblem(context, 409, "The selected seat, parking slot, or payment was already reserved.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled API error");
            await WriteProblem(context, 500, "An unexpected error occurred.");
        }
    }

    private static Task WriteProblem(HttpContext context, int status, string detail)
    {
        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(new ProblemDetails
        { Status = status, Title = ReasonPhrases.GetReasonPhrase(status), Detail = detail });
    }
}

file static class ReasonPhrases
{
    public static string GetReasonPhrase(int status) => status switch
    { 400 => "Bad Request", 404 => "Not Found", 409 => "Conflict", _ => "Server Error" };
}
