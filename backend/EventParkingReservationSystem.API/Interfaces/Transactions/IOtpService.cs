using EventParkingReservationSystem.API.DTOs.Transactions;

namespace EventParkingReservationSystem.API.Interfaces.Transactions;

public interface IOtpService
{
    Task<OtpIssuedDto> IssueAsync(
        int paymentId,
        int customerId,
        CancellationToken ct);

    Task<PaymentDto> VerifyAsync(
        OtpVerifyDto request,
        int customerId,
        CancellationToken ct);
}
