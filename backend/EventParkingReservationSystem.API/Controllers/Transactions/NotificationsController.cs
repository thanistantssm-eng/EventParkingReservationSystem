using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Extensions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Transactions;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(INotificationService service) : ControllerBase
{
    [HttpGet("me")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> MyNotifications(
        CancellationToken ct) =>
        Ok(await service.GetCustomerNotificationsAsync(
            User.RequireCustomerId(),
            ct));

    [HttpGet("customer/{customerId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> CustomerNotifications(
        int customerId,
        CancellationToken ct) =>
        Ok(await service.GetCustomerNotificationsAsync(customerId, ct));

    [HttpPut("{id:int}/read")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<NotificationDto>> MarkRead(
        int id,
        CancellationToken ct) =>
        Ok(await service.MarkReadAsync(
            id,
            User.RequireCustomerId(),
            ct));
}
