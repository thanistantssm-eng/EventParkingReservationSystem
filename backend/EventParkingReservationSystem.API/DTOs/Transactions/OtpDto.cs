using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Transactions;

public sealed class OtpRequestDto
{
    [Range(1, int.MaxValue)] public int PaymentId { get; set; }
}

public sealed class OtpVerifyDto
{
    [Range(1, int.MaxValue)] public int PaymentId { get; set; }
    [RegularExpression("^[0-9]{6}$")] public string Code { get; set; } = string.Empty;
}

public sealed record OtpIssuedDto(int PaymentId, DateTime ExpiresAtUtc, string? DevelopmentCode);
