using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Transactions;

[ApiController, Route("api/qr-codes")]
public sealed class QrCodesController(IQrCodeService service) : ControllerBase
{
    [HttpGet("booking/{bookingId:int}")]
    public async Task<ActionResult<QrCodeDto>> BookingQr(int bookingId, CancellationToken ct) => Ok(await service.GetForBookingAsync(bookingId, ct));

    [HttpGet("validate/{token}")]
    public async Task<ActionResult<QrCodeDto>> Validate(string token, CancellationToken ct) => Ok(await service.ValidateAsync(token, ct));
}
