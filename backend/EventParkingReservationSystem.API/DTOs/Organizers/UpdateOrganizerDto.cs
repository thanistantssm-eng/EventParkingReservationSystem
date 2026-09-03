using System.ComponentModel.DataAnnotations;
using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.DTOs.Organizers;

public class UpdateOrganizerDto
{
    [Required]
    public string OrganizationName { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    public string? Address { get; set; }

    public OrganizerStatus Status { get; set; }
}