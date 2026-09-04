namespace EventParkingReservationSystem.API.Interfaces.Services.Core;

public interface IEmailService
{
    Task SendLoginOtpAsync(
        string receiverEmail,
        string username,
        string otp);
}