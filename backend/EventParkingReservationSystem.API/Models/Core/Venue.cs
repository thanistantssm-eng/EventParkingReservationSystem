using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.Models.Core;

public class Venue
{
    public int Id { get; set; }

    public int PropertyId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Location { get; set; }

    public int Capacity { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Property Property { get; set; } = null!;
}