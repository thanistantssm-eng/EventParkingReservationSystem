using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingReservationSystem.API.Controllers.Transactions;

[ApiController, Route("api")]
public sealed class PaymentsController(IPaymentService service) : ControllerBase
{
    [HttpGet("bookings/{bookingId:int}/payment")]
    public async Task<ActionResult<PaymentDto>> GetForBooking(int bookingId, CancellationToken ct) =>
        (await service.GetForBookingAsync(bookingId, ct)) is { } payment ? Ok(payment) : NotFound();

    [HttpPost("bookings/{bookingId:int}/payment")]
    public async Task<ActionResult<PaymentDto>> Start(int bookingId, PaymentRequestDto request, CancellationToken ct) =>
        CreatedAtAction(nameof(GetForBooking), new { bookingId }, await service.StartAsync(bookingId, request, ct));

    [HttpGet("payments/customer/{customerId:int}")]
    public async Task<ActionResult<IReadOnlyList<PaymentDto>>> CustomerHistory(int customerId, CancellationToken ct) => Ok(await service.GetCustomerHistoryAsync(customerId, ct));

    [HttpGet("payments/{id:int}/receipt")]
    public async Task<ActionResult<PaymentReceiptDto>> Receipt(int id, CancellationToken ct) => Ok(await service.GetReceiptAsync(id, ct));
}
