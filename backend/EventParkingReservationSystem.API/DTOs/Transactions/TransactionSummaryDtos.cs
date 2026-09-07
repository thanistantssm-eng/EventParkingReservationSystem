namespace EventParkingReservationSystem.API.DTOs.Transactions;

public sealed record QrCodeDto(int BookingId, string Token, string Payload, DateTime CreatedAtUtc);
public sealed record AdminReportDto(int TotalBookings, int ConfirmedBookings, int CancelledBookings,
    int SeatsReserved, int ParkingSlotsReserved, decimal TotalRevenue);
public sealed record CustomerReportDto(int CustomerId, int TotalBookings, int UpcomingBookings,
    decimal TotalPaid, int UnreadNotifications);
public sealed record SelectionItemDto(int Id, string Label, bool IsReserved);
public sealed record EventAvailabilityDto(int EventId, string EventName, decimal TicketPrice,
    decimal ParkingFee, IReadOnlyList<SelectionItemDto> Seats, IReadOnlyList<SelectionItemDto> ParkingSlots);
