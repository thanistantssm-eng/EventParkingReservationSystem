namespace EventParkingReservationSystem.API.Models.Transactions;

public sealed class BookingTicket
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string TicketType { get; set; } = "Standard";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public Booking Booking { get; set; } = null!;
}
