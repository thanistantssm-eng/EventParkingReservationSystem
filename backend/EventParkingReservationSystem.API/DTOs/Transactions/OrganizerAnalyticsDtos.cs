namespace EventParkingReservationSystem.API.DTOs.Transactions;

public sealed record TicketTypeSalesDto(
    string TicketType,
    int TicketsSold,
    decimal Revenue);

public sealed record EventTicketSalesDto(
    int EventId,
    string EventName,
    int ConfirmedBookings,
    int TicketsSold,
    decimal TicketRevenue,
    IReadOnlyList<TicketTypeSalesDto> TicketTypes);

public sealed record OrganizerTicketSalesDto(
    int OrganizerId,
    int TotalTicketsSold,
    decimal TotalTicketRevenue,
    IReadOnlyList<EventTicketSalesDto> Events);

public sealed record OrganizerEventRevenueDto(
    int EventId,
    string EventName,
    int ConfirmedBookings,
    int TicketsSold,
    int ParkingReservations,
    decimal TicketRevenue,
    decimal ParkingRevenue,
    decimal TotalRevenue);

public sealed record EventReportTicketDto(
    string TicketType,
    int ConfiguredQuantity,
    int SoldQuantity,
    decimal Revenue);

public sealed record EventReportStatusDto(
    string Status,
    int Count);

public sealed record EventReportPaymentDto(
    int Completed,
    int Pending,
    int Failed,
    int Refunded,
    decimal Revenue,
    decimal Refunds);

public sealed record EventReportDto(
    int EventId,
    string EventName,
    string EventStatus,
    DateTime StartDateTime,
    DateTime EndDateTime,
    string Venue,
    int? OrganizerId,
    string? OrganizerName,
    string? OrganizerEmail,
    int TotalBookings,
    int ConfirmedBookings,
    int CancelledBookings,
    int CustomerCount,
    int SeatCapacity,
    int SeatsBooked,
    int ParkingCapacity,
    int ParkingBooked,
    int ParkingAvailable,
    int ParkingReservations,
    decimal TicketRevenue,
    decimal ParkingRevenue,
    decimal TotalRevenue,
    decimal Refunds,
    IReadOnlyList<EventReportTicketDto> Tickets,
    IReadOnlyList<EventReportStatusDto> BookingStatuses,
    EventReportPaymentDto Payments,
    DateTime GeneratedAtUtc);
