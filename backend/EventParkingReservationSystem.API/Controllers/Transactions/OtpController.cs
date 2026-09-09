using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Extensions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Transactions;

[ApiController]
[Route("api/otp")]
[Authorize(Roles = "Customer")]
public sealed class OtpController(IOtpService service) : ControllerBase
{
    [HttpPost("request")]
    public async Task<ActionResult<OtpIssuedDto>> RequestOtp(
        OtpRequestDto request,
        CancellationToken ct) =>
        Ok(await service.IssueAsync(
            request.PaymentId,
            User.RequireCustomerId(),
            ct));

    [HttpPost("verify")]
    public async Task<ActionResult<PaymentDto>> Verify(
        OtpVerifyDto request,
        CancellationToken ct) =>
        Ok(await service.VerifyAsync(
            request,
            User.RequireCustomerId(),
            ct));
}
