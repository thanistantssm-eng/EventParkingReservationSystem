using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Events;

public class CreateTicketTypeDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
}
