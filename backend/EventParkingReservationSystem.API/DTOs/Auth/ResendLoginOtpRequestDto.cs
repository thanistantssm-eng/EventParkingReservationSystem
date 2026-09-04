using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Auth;

public class ResendLoginOtpRequestDto
{
    [Required]
    public Guid ChallengeId { get; set; }
}