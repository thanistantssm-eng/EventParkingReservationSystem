namespace EventParkingReservationSystem.API.DTOs.Transactions;

public sealed record BookingDto(
    int Id, string BookingNumber, int CustomerId, int EventId, string EventName,
    DateTime EventStartDateTime, int? VenueId, string? ExternalVenueName,
    string Status, decimal TotalAmount, string TicketType, int Quantity,
    IReadOnlyList<string> Seats, string? ParkingSlot, string? ParkingArea,
    string? ParkingType, decimal ParkingFee, string PaymentStatus,
    DateTime CreatedAtUtc);
