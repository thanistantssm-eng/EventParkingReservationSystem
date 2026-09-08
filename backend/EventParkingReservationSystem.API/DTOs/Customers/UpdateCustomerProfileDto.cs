using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Customers;

public class UpdateCustomerProfileDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }
}