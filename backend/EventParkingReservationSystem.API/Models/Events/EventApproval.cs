using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.Models.Events;

public enum ApprovalStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public class EventApproval
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }

    public int RequestedByUserId { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    [MaxLength(1000)]
    public string? OrganizerNotes { get; set; }

    public int? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }

    [MaxLength(1000)]
    public string? ReviewReason { get; set; }
}
