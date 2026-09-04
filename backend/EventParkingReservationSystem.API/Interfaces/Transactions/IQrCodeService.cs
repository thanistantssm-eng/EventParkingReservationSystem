using EventParkingReservationSystem.API.DTOs.Transactions;

namespace EventParkingReservationSystem.API.Interfaces.Transactions;

public interface IQrCodeService
{
    Task<QrCodeDto> GetForBookingAsync(int bookingId, CancellationToken ct);
    Task<QrCodeDto> ValidateAsync(string token, CancellationToken ct);
}
