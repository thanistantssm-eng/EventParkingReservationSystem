using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Events;

public class ReviewApprovalDto
{
    [MaxLength(1000)]
    public string? Reason { get; set; }
}
