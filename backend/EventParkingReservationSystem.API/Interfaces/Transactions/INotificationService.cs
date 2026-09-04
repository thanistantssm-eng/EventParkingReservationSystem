using EventParkingReservationSystem.API.DTOs.Transactions;

namespace EventParkingReservationSystem.API.Interfaces.Transactions;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetCustomerNotificationsAsync(int customerId, CancellationToken ct);
    Task<NotificationDto> MarkReadAsync(int id, int customerId, CancellationToken ct);
}
