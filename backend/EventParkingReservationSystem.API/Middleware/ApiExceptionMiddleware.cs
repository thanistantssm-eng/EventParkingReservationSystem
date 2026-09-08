using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Middleware;

public sealed class ApiExceptionMiddleware(
    RequestDelegate next,
    ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ApiException ex)
        {
            await WriteProblem(context, ex.StatusCode, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteProblem(context, 401, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteProblem(context, 404, ex.Message);
        }
        catch (ArgumentException ex)
        {
            await WriteProblem(context, 400, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await WriteProblem(context, 409, ex.Message);
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(
                ex,
                "Database constraint rejected the request");

            await WriteProblem(
                context,
                409,
                "The request conflicts with existing database data.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled API error");

            await WriteProblem(
                context,
                500,
                "An unexpected error occurred.");
        }
    }

    private static Task WriteProblem(
        HttpContext context,
        int status,
        string detail)
    {
        context.Response.StatusCode = status;

        return context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = status,
                Title = GetReasonPhrase(status),
                Detail = detail
            });
    }

    private static string GetReasonPhrase(int status)
    {
        return status switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            _ => "Server Error"
        };
    }
}