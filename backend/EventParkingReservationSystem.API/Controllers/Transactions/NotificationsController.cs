using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Transactions;

[ApiController, Route("api/notifications")]
public sealed class NotificationsController(INotificationService service) : ControllerBase
{
    [HttpGet("customer/{customerId:int}")]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> CustomerNotifications(int customerId, CancellationToken ct) => Ok(await service.GetCustomerNotificationsAsync(customerId, ct));

    [HttpPut("{id:int}/read")]
    public async Task<ActionResult<NotificationDto>> MarkRead(int id, [FromQuery] int customerId, CancellationToken ct) => Ok(await service.MarkReadAsync(id, customerId, ct));
}
