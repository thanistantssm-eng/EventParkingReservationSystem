using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Venues;

public class VenueDto
{
    public int Id { get; set; }

    public int PropertyId { get; set; }

    public string PropertyName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Location { get; set; }

    public int Capacity { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class CreateVenueDto
{
    [Range(1, int.MaxValue)]
    public int PropertyId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Location { get; set; }

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }
}

public class UpdateVenueDto
{
    [Range(1, int.MaxValue)]
    public int PropertyId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Location { get; set; }

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }

    public bool IsActive { get; set; }
}