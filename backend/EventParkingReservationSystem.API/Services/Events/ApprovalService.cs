using EventParkingReservationSystem.API.DTOs.Events;
using EventParkingReservationSystem.API.Interfaces.Events;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Repositories.Events.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Events;

public class ApprovalService(
    IApprovalRepository approvalRepository,
    IEventRepository eventRepository,
    ITicketRepository ticketRepository,
    ISeatRepository seatRepository) : IApprovalService
{
    private readonly IApprovalRepository _approvals = approvalRepository;
    private readonly IEventRepository _events = eventRepository;
    private readonly ITicketRepository _tickets = ticketRepository;
    private readonly ISeatRepository _seats = seatRepository;

    public async Task<EventApprovalDto> SubmitAsync(int eventId, SubmitApprovalDto dto, int actorUserId, int? actorOrganizerId, string actorRole, CancellationToken cancellationToken = default)
    {
        var entity = await _events.GetByIdAsync(eventId, true, cancellationToken)
            ?? throw new KeyNotFoundException("Event not found.");

        if (!EventService.IsAdmin(actorRole) && !(EventService.IsOrganizer(actorRole) && actorOrganizerId == entity.OrganizerId))
            throw new UnauthorizedAccessException("You can submit only your own event for approval.");

        if (entity.Status != EventStatus.Draft)
            throw new InvalidOperationException(
                "Only Draft events can be submitted for approval. Rejected events must be edited and returned to Draft first.");

        if (await _approvals.GetPendingForEventAsync(eventId, cancellationToken) is not null)
            throw new InvalidOperationException("This event already has a pending approval request.");

        var tickets = await _tickets.Query().Where(x => x.EventId == eventId && x.IsActive).ToListAsync(cancellationToken);
        if (tickets.Count == 0)
            throw new InvalidOperationException("Add at least one active ticket type before submitting for approval.");

        if (entity.EventType == EventType.SeatBased)
        {
            if (!await _seats.Query().AnyAsync(x => x.EventId == eventId && x.IsActive, cancellationToken))
                throw new InvalidOperationException("SeatBased events need at least one active seat before approval.");
        }
        else if (tickets.Sum(x => x.Quantity) <= 0)
        {
            throw new InvalidOperationException("NonSeatBased events need a ticket quantity greater than zero before approval.");
        }

        var approval = new EventApproval
        {
            EventId = eventId,
            RequestedByUserId = actorUserId,
            RequestedAt = DateTime.UtcNow,
            Status = ApprovalStatus.Pending,
            OrganizerNotes = Normalize(dto.OrganizerNotes)
        };

        entity.Status = EventStatus.PendingApproval;
        entity.RejectionReason = null;
        entity.UpdatedAt = DateTime.UtcNow;

        await _approvals.AddAsync(approval, cancellationToken);
        await _approvals.SaveChangesAsync(cancellationToken);
        return await GetDtoAsync(approval.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<EventApprovalDto>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        var list = await _approvals.Query()
            .Include(x => x.Event)
            .Where(x => x.Status == ApprovalStatus.Pending)
            .OrderBy(x => x.RequestedAt)
            .ToListAsync(cancellationToken);
        return list.Select(Map).ToList();
    }

    public Task<EventApprovalDto> ApproveAsync(
        int approvalId,
        ReviewApprovalDto dto,
        int adminUserId,
        string actorRole,
        CancellationToken cancellationToken = default) =>
        ReviewAsync(
            approvalId,
            dto,
            adminUserId,
            actorRole,
            approve: true,
            cancellationToken);

    public Task<EventApprovalDto> RejectAsync(
        int approvalId,
        ReviewApprovalDto dto,
        int adminUserId,
        string actorRole,
        CancellationToken cancellationToken = default) =>
        ReviewAsync(
            approvalId,
            dto,
            adminUserId,
            actorRole,
            approve: false,
            cancellationToken);

    private async Task<EventApprovalDto> ReviewAsync(
        int approvalId,
        ReviewApprovalDto dto,
        int adminUserId,
        string actorRole,
        bool approve,
        CancellationToken cancellationToken)
    {
        if (!EventService.IsAdmin(actorRole))
        {
            throw new UnauthorizedAccessException(
                "Only Admin can approve or reject event approval requests.");
        }

        if (!approve && string.IsNullOrWhiteSpace(dto.Reason))
        {
            throw new ArgumentException(
                "Rejection reason is required.");
        }

        var approval = await _approvals.GetByIdAsync(approvalId, true, cancellationToken)
            ?? throw new KeyNotFoundException("Approval request not found.");
        if (approval.Status != ApprovalStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only Pending approval requests can be approved or rejected.");
        }

        var evt = await _events.GetByIdAsync(approval.EventId, true, cancellationToken)
            ?? throw new KeyNotFoundException("Event not found.");
        if (evt.Status != EventStatus.PendingApproval)
            throw new InvalidOperationException("The event is no longer pending approval.");

        var normalizedReason = Normalize(dto.Reason);
        var reviewedAt = DateTime.UtcNow;

        approval.Status = approve
            ? ApprovalStatus.Approved
            : ApprovalStatus.Rejected;
        approval.ReviewedByUserId = adminUserId;
        approval.ReviewedAt = reviewedAt;
        approval.ReviewReason = normalizedReason;

        evt.Status = approve
            ? EventStatus.Approved
            : EventStatus.Rejected;
        evt.ApprovedAt = approve ? reviewedAt : null;
        evt.RejectionReason = approve ? null : normalizedReason;
        evt.UpdatedAt = reviewedAt;

        await _approvals.SaveChangesAsync(cancellationToken);
        return await GetDtoAsync(approvalId, cancellationToken);
    }

    private async Task<EventApprovalDto> GetDtoAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _approvals.Query()
            .Include(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Approval request not found.");
        return Map(entity);
    }

    private static EventApprovalDto Map(EventApproval x) => new()
    {
        Id = x.Id,
        EventId = x.EventId,
        EventName = x.Event?.Name,
        RequestedByUserId = x.RequestedByUserId,
        RequestedAt = x.RequestedAt,
        Status = x.Status.ToString(),
        OrganizerNotes = x.OrganizerNotes,
        ReviewedByUserId = x.ReviewedByUserId,
        ReviewedAt = x.ReviewedAt,
        ReviewReason = x.ReviewReason
    };

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
