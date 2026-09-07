namespace EventParkingReservationSystem.API.DTOs.Transactions;

public sealed record BookingDto(
    int Id, string BookingNumber, int CustomerId, int EventId, string EventName,
    string Status, decimal TotalAmount, IReadOnlyList<string> Seats,
    string? ParkingSlot, string PaymentStatus, DateTime CreatedAtUtc);
