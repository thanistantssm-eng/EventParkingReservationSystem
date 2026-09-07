using EventParkingReservationSystem.API.Models.Core;
using EventEntity = EventParkingReservationSystem.API.Models.Events.Event;

namespace EventParkingReservationSystem.API.Models.Transactions;

public enum BookingStatus
{
    PendingPayment,
    Confirmed,
    Cancelled
}

public sealed class Booking
{
    public int Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public int EventId { get; set; }

    public BookingStatus Status { get; set; } =
        BookingStatus.PendingPayment;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAtUtc { get; set; } =
        DateTime.UtcNow;

    public DateTime? CancelledAtUtc { get; set; }

    public Customer Customer { get; set; } = null!;

    // Canonical event entity is Member 2's Models.Events.Event.
    public EventEntity Event { get; set; } = null!;

    public ICollection<BookingTicket> Tickets { get; set; } = [];
    public ICollection<BookingSeat> Seats { get; set; } = [];

    public BookingParking? Parking { get; set; }
    public Payment? Payment { get; set; }
    public QrCode? QrCode { get; set; }
}
