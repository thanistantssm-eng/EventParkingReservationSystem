using EventParkingReservationSystem.API.Exceptions;

namespace EventParkingReservationSystem.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<ExceptionMiddleware>
        _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            context.Response.StatusCode =
                ex.StatusCode;

            context.Response.ContentType =
                "application/json";

            await context.Response
                .WriteAsJsonAsync(new
                {
                    message = ex.Message
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled server error.");

            context.Response.StatusCode =
                StatusCodes
                    .Status500InternalServerError;

            context.Response.ContentType =
                "application/json";

            await context.Response
                .WriteAsJsonAsync(new
                {
                    message =
                        "Internal server error."
                });
        }
    }
}