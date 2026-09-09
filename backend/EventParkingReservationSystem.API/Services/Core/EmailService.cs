using System.Text.Encodings.Web;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using MailKit.Security;
using MimeKit;

namespace EventParkingReservationSystem.API.Services.Core;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task SendLoginOtpAsync(
        string receiverEmail,
        string username,
        string otp) =>
        SendOtpEmailAsync(
            receiverEmail,
            username,
            otp,
            "Login Verification OTP",
            "Your login verification code is:",
            "If you did not try to login, you can ignore this email.");

    public Task SendPaymentOtpAsync(
        string receiverEmail,
        string customerName,
        string otp,
        string bookingNumber) =>
        SendOtpEmailAsync(
            receiverEmail,
            customerName,
            otp,
            "Payment Verification OTP",
            $"Your payment verification code for booking {bookingNumber} is:",
            "If you did not start this payment, please do not share this OTP.");

    public Task SendBookingExpiredAsync(
        string receiverEmail,
        string customerName,
        string bookingNumber,
        int holdMinutes)
    {
        var safeName = HtmlEncoder.Default.Encode(customerName);
        var safeBookingNumber = HtmlEncoder.Default.Encode(bookingNumber);

        var htmlBody = $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family:Arial,sans-serif;background:#f4f4f4;padding:30px;">
                <div style="max-width:560px;margin:auto;background:white;padding:30px;border-radius:10px;">
                    <h2>Event Parking Reservation System</h2>
                    <p>Hello {safeName},</p>
                    <p>
                        Your booking <strong>{safeBookingNumber}</strong> was automatically
                        cancelled because payment was not completed within
                        <strong>{holdMinutes} minutes</strong>.
                    </p>
                    <p>Your reserved seats and parking have been released and are available for booking again.</p>
                    <p>If you still want to attend the event, please create a new booking.</p>
                </div>
            </body>
            </html>
            """;

        return SendHtmlEmailAsync(
            receiverEmail,
            $"Booking expired - {bookingNumber}",
            htmlBody);
    }

    private Task SendOtpEmailAsync(
        string receiverEmail,
        string displayName,
        string otp,
        string subject,
        string intro,
        string footer)
    {
        var safeName = HtmlEncoder.Default.Encode(displayName);
        var safeIntro = HtmlEncoder.Default.Encode(intro);
        var safeOtp = HtmlEncoder.Default.Encode(otp);
        var safeFooter = HtmlEncoder.Default.Encode(footer);

        var htmlBody = $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family:Arial,sans-serif;background:#f4f4f4;padding:30px;">
                <div style="max-width:500px;margin:auto;background:white;padding:30px;border-radius:10px;">
                    <h2>Event Parking Reservation System</h2>
                    <p>Hello {safeName},</p>
                    <p>{safeIntro}</p>
                    <div style="font-size:32px;font-weight:bold;letter-spacing:8px;margin:25px 0;">
                        {safeOtp}
                    </div>
                    <p>This OTP will expire in <strong>5 minutes</strong>.</p>
                    <p>{safeFooter}</p>
                </div>
            </body>
            </html>
            """;

        return SendHtmlEmailAsync(
            receiverEmail,
            subject,
            htmlBody);
    }

    private async Task SendHtmlEmailAsync(
        string receiverEmail,
        string subject,
        string htmlBody)
    {
        var smtpHost =
            _configuration["Email:SmtpHost"]
            ?? throw new InvalidOperationException("SMTP host is not configured.");

        var smtpPort = int.Parse(
            _configuration["Email:SmtpPort"] ?? "587");

        var smtpUsername =
            _configuration["Email:Username"]
            ?? throw new InvalidOperationException("SMTP username is not configured.");

        var smtpPassword =
            _configuration["Email:Password"]
            ?? throw new InvalidOperationException("SMTP password is not configured.");

        if (string.IsNullOrWhiteSpace(smtpUsername) ||
            string.IsNullOrWhiteSpace(smtpPassword))
        {
            throw new InvalidOperationException(
                "Email credentials are not configured.");
        }

        var configuredFromAddress =
            _configuration["Email:FromAddress"];

        var fromAddress = string.IsNullOrWhiteSpace(configuredFromAddress)
            ? smtpUsername
            : configuredFromAddress;

        var fromName =
            _configuration["Email:FromName"]
            ?? "Event Parking Reservation System";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(MailboxAddress.Parse(receiverEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var smtp = new MailKit.Net.Smtp.SmtpClient();
        await smtp.ConnectAsync(
            smtpHost,
            smtpPort,
            SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(smtpUsername, smtpPassword);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }
}
