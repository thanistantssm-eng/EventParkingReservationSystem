namespace EventParkingReservationSystem.API.DTOs.Events;

public sealed record FavoriteEventDto(
    int Id,
    int EventId,
    string EventName,
    string? PosterUrl,
    string? CategoryName,
    DateTime StartDateTime,
    decimal TicketPrice,
    DateTime CreatedAtUtc);
