using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Middleware;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Transactions;

public sealed class NotificationService(AppDbContext db) : INotificationService
{
    public async Task<IReadOnlyList<NotificationDto>> GetCustomerNotificationsAsync(int customerId, CancellationToken ct) =>
        (await db.Notifications.AsNoTracking().Where(x => x.CustomerId == customerId).OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct)).Select(Map).ToList();

    public async Task<NotificationDto> MarkReadAsync(int id, int customerId, CancellationToken ct)
    {
        var item = await db.Notifications.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Notification not found.");
        if (item.CustomerId != customerId) throw new ApiException(403, "You can only update your own notifications.");
        item.IsRead = true;
        await db.SaveChangesAsync(ct);
        return Map(item);
    }

    private static NotificationDto Map(Models.Transactions.Notification x) => new(x.Id, x.CustomerId, x.BookingId, x.Title, x.Message, x.IsRead, x.CreatedAtUtc);
}
