using EventParkingReservationSystem.API.DTOs.Notifications;
using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.Interfaces.Services.Core;

public interface INotificationService
{
    Task<List<NotificationDto>> GetForUserAsync(
        int userId);

    Task<int> GetUnreadCountAsync(
        int userId);

    Task<NotificationDto?> MarkAsReadAsync(
        int notificationId,
        int userId);

    Task MarkAllAsReadAsync(
        int userId);

    Task<NotificationDto> SendToUserAsync(
        int userId,
        string title,
        string message,
        string type = "General");

    Task SendToRoleAsync(
        UserRole role,
        string title,
        string message,
        string type = "General");
}