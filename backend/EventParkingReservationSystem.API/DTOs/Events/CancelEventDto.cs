using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Events;

public class CancelEventDto
{
    [MaxLength(500)]
    public string? Reason { get; set; }
}
