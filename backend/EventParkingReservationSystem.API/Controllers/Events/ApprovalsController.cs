using System.Security.Claims;
using EventParkingReservationSystem.API.DTOs.Events;
using EventParkingReservationSystem.API.Interfaces.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Events;

[ApiController]
[Route("api/approvals")]
[Authorize]
public class ApprovalsController(IApprovalService service) : ControllerBase
{
    private readonly IApprovalService _service = service;

    [Authorize(Roles = "Admin,Organizer")]
    [HttpPost("events/{eventId:int}/submit")]
    public async Task<ActionResult<EventApprovalDto>> Submit(int eventId, SubmitApprovalDto dto, CancellationToken cancellationToken) =>
        Ok(await _service.SubmitAsync(eventId, dto, UserId(), OrganizerId(), Role(), cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpGet("pending")]
    public async Task<ActionResult<IReadOnlyList<EventApprovalDto>>> GetPending(CancellationToken cancellationToken) =>
        Ok(await _service.GetPendingAsync(cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPut("{approvalId:int}/approve")]
    public async Task<ActionResult<EventApprovalDto>> Approve(int approvalId, ReviewApprovalDto dto, CancellationToken cancellationToken) =>
        Ok(await _service.ApproveAsync(approvalId, dto, UserId(), cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPut("{approvalId:int}/reject")]
    public async Task<ActionResult<EventApprovalDto>> Reject(int approvalId, ReviewApprovalDto dto, CancellationToken cancellationToken) =>
        Ok(await _service.RejectAsync(approvalId, dto, UserId(), cancellationToken));

    private int UserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? User.FindFirstValue("userId");
        return int.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException("User id claim is missing.");
    }

    private int? OrganizerId()
    {
        var value = User.FindFirstValue("organizerId") ?? User.FindFirstValue("OrganizerId");
        return int.TryParse(value, out var id) ? id : null;
    }

    private string Role() => User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role") ?? string.Empty;

}
