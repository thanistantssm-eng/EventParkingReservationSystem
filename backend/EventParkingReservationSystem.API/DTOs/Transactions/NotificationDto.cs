namespace EventParkingReservationSystem.API.DTOs.Transactions;

public sealed record NotificationDto(int Id, int CustomerId, int? BookingId, string Title,
    string Message, bool IsRead, DateTime CreatedAtUtc);
