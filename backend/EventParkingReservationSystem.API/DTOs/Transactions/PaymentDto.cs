using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.DTOs.Transactions;

public sealed class PaymentRequestDto
{
    [Required, MaxLength(30)] public string Method { get; set; } = "Card";
}

public sealed record PaymentDto(int Id, int BookingId, decimal Amount, string Method,
    string Status, string TransactionReference, DateTime CreatedAtUtc, DateTime? CompletedAtUtc);

public sealed record PaymentReceiptDto(string ReceiptNumber, string BookingNumber,
    decimal Amount, string Method, string TransactionReference, DateTime PaidAtUtc);
