namespace EventParkingReservationSystem.API.Interfaces.Services.Core;

public interface IEmailService
{
    Task SendLoginOtpAsync(
        string receiverEmail,
        string username,
        string otp);

    Task SendPaymentOtpAsync(
        string receiverEmail,
        string customerName,
        string otp,
        string bookingNumber);

    Task SendBookingExpiredAsync(
        string receiverEmail,
        string customerName,
        string bookingNumber,
        int holdMinutes);
}
