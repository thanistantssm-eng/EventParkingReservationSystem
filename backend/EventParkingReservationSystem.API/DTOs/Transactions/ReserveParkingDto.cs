using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Transactions;

public sealed class ReserveParkingDto
{
    [Range(1, int.MaxValue)]
    public int ParkingSlotId { get; set; }
}
