using EventParkingReservationSystem.API.DTOs.Transactions;

namespace EventParkingReservationSystem.API.Interfaces.Transactions;

public interface IPaymentService
{
    Task<PaymentDto?> GetForBookingAsync(int bookingId, CancellationToken ct);
    Task<PaymentDto> StartAsync(int bookingId, PaymentRequestDto request, CancellationToken ct);
    Task<PaymentDto> CompleteAsync(int paymentId, CancellationToken ct);
    Task<IReadOnlyList<PaymentDto>> GetCustomerHistoryAsync(int customerId, CancellationToken ct);
    Task<PaymentReceiptDto> GetReceiptAsync(int paymentId, CancellationToken ct);
}
