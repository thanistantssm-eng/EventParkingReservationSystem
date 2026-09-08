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