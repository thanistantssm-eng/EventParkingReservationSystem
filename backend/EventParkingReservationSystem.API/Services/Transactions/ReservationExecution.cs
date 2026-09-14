using System.Data;
using EventParkingReservationSystem.API.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Transactions;

internal static class ReservationExecution
{
    // Retry the entire unit, including BeginTransaction, not individual SQL
    // statements inside a caller-managed transaction.
    public static Task<T> RunAsync<T>(AppDbContext db, Func<Task<T>> operation)
    {
        var attempts = 0;
        return db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            if (attempts++ > 0)
            {
                db.ChangeTracker.Clear();
            }

            return await operation();
        });
    }

    public static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.GetBaseException() is SqlException { Number: 2601 or 2627 };

    public static Task<T> InTransactionAsync<T>(
        AppDbContext db,
        Func<Task<T>> operation,
        CancellationToken ct) => RunAsync(db, async () =>
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, ct);
            var result = await operation();
            await transaction.CommitAsync(ct);
            return result;
        });
}
