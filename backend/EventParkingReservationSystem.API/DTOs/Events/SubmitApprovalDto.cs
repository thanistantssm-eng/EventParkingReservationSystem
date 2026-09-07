using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Events;

public class SubmitApprovalDto
{
    [MaxLength(1000)]
    public string? OrganizerNotes { get; set; }
}

public class EventApprovalDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string? EventName { get; set; }
    public int RequestedByUserId { get; set; }
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? OrganizerNotes { get; set; }
    public int? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewReason { get; set; }
}
