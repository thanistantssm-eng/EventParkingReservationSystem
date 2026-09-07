using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Transactions;

[ApiController, Route("api/otp")]
public sealed class OtpController(IOtpService service) : ControllerBase
{
    [HttpPost("request")]
    public async Task<ActionResult<OtpIssuedDto>> RequestOtp(OtpRequestDto request, CancellationToken ct) => Ok(await service.IssueAsync(request.PaymentId, ct));

    [HttpPost("verify")]
    public async Task<ActionResult<PaymentDto>> Verify(OtpVerifyDto request, CancellationToken ct) => Ok(await service.VerifyAsync(request, ct));
}
