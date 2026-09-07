using System.Security.Claims;
using EventParkingReservationSystem.API.DTOs.Notifications;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Core;

[ApiController]
[Route("api/user-notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService
        _notificationService;

    public NotificationsController(
        INotificationService notificationService)
    {
        _notificationService =
            notificationService;
    }

    // Current logged-in user's notifications
    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var userId = CurrentUserId();

        var notifications =
            await _notificationService
                .GetForUserAsync(userId);

        return Ok(notifications);
    }

    // Current user's unread notification count
    [HttpGet("unread-count")]
    public async Task<IActionResult>
        UnreadCount()
    {
        var count =
            await _notificationService
                .GetUnreadCountAsync(
                    CurrentUserId());

        return Ok(new
        {
            unreadCount = count
        });
    }

    // Mark one notification as read
    [HttpPut("{id:int}/read")]
    public async Task<IActionResult>
        MarkAsRead(int id)
    {
        var result =
            await _notificationService
                .MarkAsReadAsync(
                    id,
                    CurrentUserId());

        return result == null
            ? NotFound()
            : Ok(result);
    }

    // Mark all as read
    [HttpPut("read-all")]
    public async Task<IActionResult>
        MarkAllAsRead()
    {
        await _notificationService
            .MarkAllAsReadAsync(
                CurrentUserId());

        return Ok(new
        {
            message =
                "All notifications marked as read."
        });
    }

    // Admin can manually send notification
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Send(
        CreateNotificationDto request)
    {
        try
        {
            var result =
                await _notificationService
                    .SendToUserAsync(
                        request.UserId,
                        request.Title,
                        request.Message,
                        request.Type);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    private int CurrentUserId()
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(value, out var id))
        {
            throw new UnauthorizedAccessException(
                "Invalid user token.");
        }

        return id;
    }
}