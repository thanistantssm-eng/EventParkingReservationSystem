using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Notifications;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Core;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationDto>>
        GetForUserAsync(int userId)
    {
        return await _context.UserNotifications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new NotificationDto
            {
                Id = x.Id,
                UserId = x.UserId,
                Title = x.Title,
                Message = x.Message,
                Type = x.Type,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt,
                ReadAt = x.ReadAt
            })
            .ToListAsync();
    }

    public async Task<int>
        GetUnreadCountAsync(int userId)
    {
        return await _context.UserNotifications
            .CountAsync(x =>
                x.UserId == userId &&
                !x.IsRead);
    }

    public async Task<NotificationDto?>
        MarkAsReadAsync(
            int notificationId,
            int userId)
    {
        var notification =
            await _context.UserNotifications
                .FirstOrDefaultAsync(x =>
                    x.Id == notificationId &&
                    x.UserId == userId);

        if (notification == null)
        {
            return null;
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;

            notification.ReadAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        return Map(notification);
    }

    public async Task MarkAllAsReadAsync(
        int userId)
    {
        var notifications =
            await _context.UserNotifications
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsRead)
                .ToListAsync();

        var now = DateTime.UtcNow;

        foreach (var notification
                 in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<NotificationDto>
        SendToUserAsync(
            int userId,
            string title,
            string message,
            string type = "General")
    {
        var userExists =
            await _context.Users
                .AnyAsync(x =>
                    x.Id == userId);

        if (!userExists)
        {
            throw new InvalidOperationException(
                "User not found.");
        }

        var notification =
            new UserNotification
            {
                UserId = userId,

                Title = title.Trim(),

                Message = message.Trim(),

                Type =
                    string.IsNullOrWhiteSpace(type)
                        ? "General"
                        : type.Trim(),

                IsRead = false,

                CreatedAt = DateTime.UtcNow
            };

        await _context.UserNotifications
            .AddAsync(notification);

        await _context.SaveChangesAsync();

        return Map(notification);
    }

    public async Task SendToRoleAsync(
        UserRole role,
        string title,
        string message,
        string type = "General")
    {
        var userIds =
            await _context.Users
                .Where(x =>
                    x.Role == role &&
                    x.IsActive)
                .Select(x => x.Id)
                .ToListAsync();

        if (userIds.Count == 0)
        {
            return;
        }

        var notifications =
            userIds.Select(userId =>
                new UserNotification
                {
                    UserId = userId,

                    Title = title.Trim(),

                    Message = message.Trim(),

                    Type =
                        string.IsNullOrWhiteSpace(type)
                            ? "General"
                            : type.Trim(),

                    IsRead = false,

                    CreatedAt =
                        DateTime.UtcNow
                })
                .ToList();

        await _context.UserNotifications
            .AddRangeAsync(notifications);

        await _context.SaveChangesAsync();
    }

    private static NotificationDto Map(
        UserNotification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,

            UserId = notification.UserId,

            Title = notification.Title,

            Message = notification.Message,

            Type = notification.Type,

            IsRead = notification.IsRead,

            CreatedAt =
                notification.CreatedAt,

            ReadAt =
                notification.ReadAt
        };
    }
}