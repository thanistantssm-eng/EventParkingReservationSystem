using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Extensions;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Controllers.Transactions;

[ApiController]
[Route("api")]
[Authorize]
public sealed class PaymentsController(
    IPaymentService service,
    AppDbContext db) : ControllerBase
{
    [HttpGet("bookings/{bookingId:int}/payment")]
    [Authorize(Roles = "Admin,Organizer,Customer")]
    public async Task<ActionResult<PaymentDto>> GetForBooking(
        int bookingId,
        CancellationToken ct)
    {
        await EnsureBookingAccessAsync(bookingId, ct);
        return (await service.GetForBookingAsync(bookingId, ct)) is { } payment
            ? Ok(payment)
            : NotFound();
    }

    [HttpPost("bookings/{bookingId:int}/payment")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<PaymentDto>> Start(
        int bookingId,
        PaymentRequestDto request,
        CancellationToken ct)
    {
        await EnsureCustomerOwnsBookingAsync(bookingId, ct);

        return CreatedAtAction(
            nameof(GetForBooking),
            new { bookingId },
            await service.StartAsync(bookingId, request, ct));
    }

    [HttpGet("payments/me")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<IReadOnlyList<PaymentDto>>> MyHistory(
        CancellationToken ct) =>
        Ok(await service.GetCustomerHistoryAsync(
            User.RequireCustomerId(),
            ct));

    [HttpGet("payments/customer/{customerId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<PaymentDto>>> CustomerHistory(
        int customerId,
        CancellationToken ct) =>
        Ok(await service.GetCustomerHistoryAsync(customerId, ct));

    [HttpGet("payments/{id:int}/receipt")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<ActionResult<PaymentReceiptDto>> Receipt(
        int id,
        CancellationToken ct)
    {
        await EnsurePaymentAccessAsync(id, ct);
        return Ok(await service.GetReceiptAsync(id, ct));
    }

    [HttpPost("payments/{id:int}/refund")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PaymentDto>> Refund(
        int id,
        CancellationToken ct) =>
        Ok(await service.RefundAsync(id, ct));

    private async Task EnsureCustomerOwnsBookingAsync(
        int bookingId,
        CancellationToken ct)
    {
        var customerId = User.RequireCustomerId();
        var ownsBooking = await db.Bookings
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == bookingId && x.CustomerId == customerId,
                ct);

        if (!ownsBooking)
        {
            throw new ApiException(403, "You can only pay for your own booking.");
        }
    }

    private async Task EnsureBookingAccessAsync(
        int bookingId,
        CancellationToken ct)
    {
        var role = User.RequireRole();
        if (role == "Admin") return;

        var allowed = role switch
        {
            "Customer" => await db.Bookings
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == bookingId &&
                         x.CustomerId == User.RequireCustomerId(),
                    ct),

            "Organizer" => await db.Bookings
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == bookingId &&
                         x.Event.OrganizerId == User.RequireOrganizerId(),
                    ct),

            _ => false
        };

        if (!allowed)
        {
            throw new ApiException(403, "You do not have access to this payment.");
        }
    }

    private async Task EnsurePaymentAccessAsync(
        int paymentId,
        CancellationToken ct)
    {
        if (User.RequireRole() == "Admin") return;

        var customerId = User.RequireCustomerId();
        var allowed = await db.Payments
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == paymentId &&
                     x.Booking.CustomerId == customerId,
                ct);

        if (!allowed)
        {
            throw new ApiException(403, "You do not have access to this receipt.");
        }
    }
}
