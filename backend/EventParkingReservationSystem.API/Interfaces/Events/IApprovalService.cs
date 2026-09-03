using EventParkingReservationSystem.API.DTOs.Events;

namespace EventParkingReservationSystem.API.Interfaces.Events;

public interface IApprovalService
{
    Task<EventApprovalDto> SubmitAsync(int eventId, SubmitApprovalDto dto, int actorUserId, int? actorOrganizerId, string actorRole, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventApprovalDto>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<EventApprovalDto> ApproveAsync(int approvalId, ReviewApprovalDto dto, int adminUserId, CancellationToken cancellationToken = default);
    Task<EventApprovalDto> RejectAsync(int approvalId, ReviewApprovalDto dto, int adminUserId, CancellationToken cancellationToken = default);
}
