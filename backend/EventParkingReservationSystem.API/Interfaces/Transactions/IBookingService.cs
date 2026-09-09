using EventParkingReservationSystem.API.DTOs.Transactions;

namespace EventParkingReservationSystem.API.Interfaces.Transactions;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(CreateBookingDto request, CancellationToken ct);
    Task<BookingDto> GetAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<BookingDto>> GetCustomerBookingsAsync(int customerId, CancellationToken ct);
    Task<IReadOnlyList<BookingDto>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<BookingDto>> GetEventBookingsAsync(int eventId, CancellationToken ct);
    Task<EventAvailabilityDto> GetAvailabilityAsync(int eventId, CancellationToken ct);
    Task<BookingDto> AttachParkingAsync(int id, int customerId, ReserveParkingDto request, CancellationToken ct);
    Task RemoveParkingAsync(int id, int customerId, CancellationToken ct);
    Task CancelAsync(int id, int customerId, CancellationToken ct);
}
