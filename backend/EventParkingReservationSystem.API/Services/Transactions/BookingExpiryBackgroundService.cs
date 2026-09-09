using EventParkingReservationSystem.API.Interfaces.Transactions;

namespace EventParkingReservationSystem.API.Services.Transactions;

/// <summary>
/// Automatically expires unpaid bookings in the background, even when no
/// customer is actively calling the API.
/// </summary>
public sealed class BookingExpiryBackgroundService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<BookingExpiryBackgroundService> logger) : BackgroundService
{
    private TimeSpan CleanupInterval => TimeSpan.FromSeconds(
        Math.Clamp(
            configuration.GetValue<int?>("BookingHold:CleanupIntervalSeconds") ?? 60,
            10,
            3600));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Booking expiry background worker started. Cleanup interval: {IntervalSeconds} seconds.",
            CleanupInterval.TotalSeconds);

        // Run once at startup so bookings that expired while the API was
        // stopped are cleaned as soon as the application starts.
        await RunCleanupAsync(stoppingToken);

        using var timer = new PeriodicTimer(CleanupInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunCleanupAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal application shutdown.
        }
    }

    private async Task RunCleanupAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();

            var expiryService = scope.ServiceProvider
                .GetRequiredService<IBookingExpiryService>();

            await expiryService.ExpireStalePendingBookingsAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal application shutdown.
        }
        catch (Exception ex)
        {
            // Keep the worker alive. A temporary database/SMTP problem must not
            // permanently stop future expiry checks.
            logger.LogError(
                ex,
                "Booking expiry background cleanup failed. It will be retried on the next interval.");
        }
    }
}
