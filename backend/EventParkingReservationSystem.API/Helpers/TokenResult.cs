namespace EventParkingReservationSystem.API.Helpers;

public record TokenResult(
    string Token,
    DateTime ExpiresAtUtc);