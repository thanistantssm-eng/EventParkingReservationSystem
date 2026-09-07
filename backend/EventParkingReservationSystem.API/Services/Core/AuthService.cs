using System.Security.Cryptography;
using System.Text;
using EventParkingReservationSystem.API.DTOs.Auth;
using EventParkingReservationSystem.API.Interfaces.Repositories.Core;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.AspNetCore.Identity;

namespace EventParkingReservationSystem.API.Services.Core;

public class AuthService : IAuthService
{
    private readonly IUserRepository
        _userRepository;

    private readonly ILoginOtpRepository
        _loginOtpRepository;

    private readonly IJwtTokenService
        _jwtTokenService;

    private readonly IEmailService
        _emailService;

    private readonly IConfiguration
        _configuration;

    private readonly INotificationService
        _notificationService;

    private readonly PasswordHasher<User>
        _passwordHasher = new();


    public AuthService(
        IUserRepository userRepository,
        ILoginOtpRepository loginOtpRepository,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        IConfiguration configuration,
        INotificationService notificationService)
    {
        _userRepository =
            userRepository;

        _loginOtpRepository =
            loginOtpRepository;

        _jwtTokenService =
            jwtTokenService;

        _emailService =
            emailService;

        _configuration =
            configuration;

        _notificationService =
            notificationService;
    }


    // ============================================
    // REGISTER
    // ============================================

    public async Task<RegisterResponseDto>
        RegisterAsync(
            RegisterRequestDto request)
    {
        var username =
            request.Username.Trim();

        var email =
            request.Email
                .Trim()
                .ToLowerInvariant();

        if (await _userRepository
            .UsernameExistsAsync(username))
        {
            throw new InvalidOperationException(
                "Username already exists.");
        }

        if (await _userRepository
            .EmailExistsAsync(email))
        {
            throw new InvalidOperationException(
                "Email already exists.");
        }


        if (!Enum.TryParse<UserRole>(
                request.Role,
                true,
                out var role))
        {
            throw new InvalidOperationException(
                "Invalid role.");
        }


        // Public registration must never create Admin.
        if (role == UserRole.Admin)
        {
            throw new InvalidOperationException(
                "Admin cannot register publicly.");
        }


        if (role == UserRole.Organizer &&
            string.IsNullOrWhiteSpace(
                request.OrganizationName))
        {
            throw new InvalidOperationException(
                "Organization name is required.");
        }


        var user = new User
        {
            Username = username,

            Email = email,

            Role = role,

            IsActive = true,

            CreatedAt =
                DateTime.UtcNow
        };


        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                request.Password);


        // ============================================
        // ORGANIZER PROFILE
        // ============================================

        if (role == UserRole.Organizer)
        {
            user.Organizer =
                new Organizer
                {
                    OrganizationName =
                        request
                            .OrganizationName!
                            .Trim(),

                    PhoneNumber =
                        Clean(
                            request.PhoneNumber),

                    Address =
                        Clean(
                            request.Address),

                    IsVerified = false,

                    CreatedAt =
                        DateTime.UtcNow
                };
        }


        // ============================================
        // CUSTOMER PROFILE
        // ============================================

        if (role == UserRole.Customer)
        {
            user.Customer =
                new Customer
                {
                    Name =
                        username,

                    Email =
                        email,

                    Phone =
                        Clean(
                            request.PhoneNumber),

                    CreatedAt =
                        DateTime.UtcNow
                };
        }


        await _userRepository
            .AddAsync(user);

        await _userRepository
            .SaveChangesAsync();


        // ============================================
        // WELCOME NOTIFICATION
        // ============================================

        await _notificationService
            .SendToUserAsync(
                user.Id,
                "Welcome",
                $"Welcome {user.Username}. Your account was created successfully.",
                "Account");


        // ============================================
        // ORGANIZER REGISTER ->
        // NOTIFY ALL ADMINS
        // ============================================

        if (user.Role ==
            UserRole.Organizer)
        {
            await _notificationService
                .SendToRoleAsync(
                    UserRole.Admin,

                    "New Organizer Registered",

                    $"{user.Username} registered as an organizer and is waiting for verification.",

                    "OrganizerRegistration");
        }


        return new RegisterResponseDto
        {
            UserId =
                user.Id,

            Username =
                user.Username,

            Email =
                user.Email,

            Role =
                user.Role.ToString()
        };
    }


    // ============================================
    // LOGIN
    // ============================================

    public async Task<LoginPendingResponseDto>
        LoginAsync(
            LoginRequestDto request)
    {
        var user =
            await _userRepository
                .GetByIdentifierAsync(
                    request.Identifier);

        if (user == null)
        {
            throw new UnauthorizedAccessException(
                "Invalid username/email or password.");
        }


        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Account is inactive.");
        }


        var passwordResult =
            _passwordHasher
                .VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password);


        if (passwordResult ==
            PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException(
                "Invalid username/email or password.");
        }


        await _loginOtpRepository
            .InvalidateActiveOtpsAsync(
                user.Id);


        return await
            CreateAndSendOtpAsync(user);
    }


    // ============================================
    // VERIFY LOGIN OTP
    // ============================================

    public async Task<AuthResponseDto>
        VerifyLoginOtpAsync(
            VerifyLoginOtpRequestDto request)
    {
        var loginOtp =
            await _loginOtpRepository
                .GetByChallengeIdAsync(
                    request.ChallengeId);


        if (loginOtp == null)
        {
            throw new UnauthorizedAccessException(
                "Invalid OTP session.");
        }


        if (loginOtp.IsUsed)
        {
            throw new UnauthorizedAccessException(
                "OTP has already been used.");
        }


        if (loginOtp.ExpiresAt <
            DateTime.UtcNow)
        {
            loginOtp.IsUsed = true;

            loginOtp.UsedAt =
                DateTime.UtcNow;

            await _loginOtpRepository
                .SaveChangesAsync();

            throw new UnauthorizedAccessException(
                "OTP has expired.");
        }


        if (loginOtp.FailedAttempts >= 5)
        {
            loginOtp.IsUsed = true;

            loginOtp.UsedAt =
                DateTime.UtcNow;

            await _loginOtpRepository
                .SaveChangesAsync();

            throw new UnauthorizedAccessException(
                "Maximum OTP attempts exceeded.");
        }


        if (!VerifyOtp(
                request.Otp,
                loginOtp.OtpHash))
        {
            loginOtp.FailedAttempts++;


            if (loginOtp.FailedAttempts >= 5)
            {
                loginOtp.IsUsed = true;

                loginOtp.UsedAt =
                    DateTime.UtcNow;
            }


            await _loginOtpRepository
                .SaveChangesAsync();


            throw new UnauthorizedAccessException(
                "Invalid OTP.");
        }


        var user =
            loginOtp.User;


        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Account is inactive.");
        }


        loginOtp.IsUsed = true;

        loginOtp.UsedAt =
            DateTime.UtcNow;


        await _loginOtpRepository
            .SaveChangesAsync();


        var expiresAt =
            _jwtTokenService
                .GetExpirationTime();


        var token =
            _jwtTokenService
                .GenerateToken(user);


        return new AuthResponseDto
        {
            Token =
                token,

            ExpiresAt =
                expiresAt,

            UserId =
                user.Id,

            Username =
                user.Username,

            Email =
                user.Email,

            Role =
                user.Role.ToString(),

            OrganizerId =
                user.Organizer?.Id,

            CustomerId =
                user.Customer?.Id
        };
    }


    // ============================================
    // RESEND LOGIN OTP
    // ============================================

    public async Task<LoginPendingResponseDto>
        ResendLoginOtpAsync(
            ResendLoginOtpRequestDto request)
    {
        var oldOtp =
            await _loginOtpRepository
                .GetByChallengeIdAsync(
                    request.ChallengeId);


        if (oldOtp == null)
        {
            throw new UnauthorizedAccessException(
                "Invalid OTP session.");
        }


        if (oldOtp.IsUsed)
        {
            throw new UnauthorizedAccessException(
                "OTP session is no longer active.");
        }


        if (!oldOtp.User.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Account is inactive.");
        }


        var elapsedSeconds =
            (DateTime.UtcNow -
             oldOtp.CreatedAt)
            .TotalSeconds;


        if (elapsedSeconds < 60)
        {
            throw new InvalidOperationException(
                "Please wait 60 seconds before requesting another OTP.");
        }


        await _loginOtpRepository
            .InvalidateActiveOtpsAsync(
                oldOtp.UserId);


        return await
            CreateAndSendOtpAsync(
                oldOtp.User);
    }


    // ============================================
    // CREATE + SEND OTP
    // ============================================

    private async Task<LoginPendingResponseDto>
        CreateAndSendOtpAsync(
            User user)
    {
        var otp =
            RandomNumberGenerator
                .GetInt32(
                    100000,
                    1000000)
                .ToString();


        var challengeId =
            Guid.NewGuid();


        var expiresAt =
            DateTime.UtcNow
                .AddMinutes(5);


        var loginOtp =
            new LoginOtp
            {
                ChallengeId =
                    challengeId,

                UserId =
                    user.Id,

                OtpHash =
                    HashOtp(otp),

                CreatedAt =
                    DateTime.UtcNow,

                ExpiresAt =
                    expiresAt,

                FailedAttempts = 0,

                IsUsed = false
            };


        await _loginOtpRepository
            .AddAsync(loginOtp);


        await _loginOtpRepository
            .SaveChangesAsync();


        try
        {
            await _emailService
                .SendLoginOtpAsync(
                    user.Email,
                    user.Username,
                    otp);
        }
        catch
        {
            loginOtp.IsUsed = true;

            loginOtp.UsedAt =
                DateTime.UtcNow;


            await _loginOtpRepository
                .SaveChangesAsync();

            throw;
        }


        return new LoginPendingResponseDto
        {
            RequiresOtp = true,

            ChallengeId =
                challengeId,

            MaskedEmail =
                MaskEmail(
                    user.Email),

            ExpiresAt =
                expiresAt
        };
    }


    // ============================================
    // OTP HASH
    // ============================================

    private string HashOtp(
        string otp)
    {
        var pepper =
            _configuration[
                "Otp:Pepper"]
            ?? throw new InvalidOperationException(
                "Otp:Pepper is missing.");


        using var hmac =
            new HMACSHA256(
                Encoding.UTF8
                    .GetBytes(pepper));


        return Convert.ToHexString(
            hmac.ComputeHash(
                Encoding.UTF8
                    .GetBytes(otp)));
    }


    // ============================================
    // VERIFY OTP HASH
    // ============================================

    private bool VerifyOtp(
        string otp,
        string expectedHash)
    {
        try
        {
            var actual =
                Convert.FromHexString(
                    HashOtp(otp));


            var expected =
                Convert.FromHexString(
                    expectedHash);


            return CryptographicOperations
                .FixedTimeEquals(
                    actual,
                    expected);
        }
        catch
        {
            return false;
        }
    }


    // ============================================
    // MASK EMAIL
    // ============================================

    private static string MaskEmail(
        string email)
    {
        var index =
            email.IndexOf('@');


        if (index <= 0)
        {
            return "***";
        }


        var local =
            email[..index];


        var domain =
            email[
                (index + 1)..];


        var visible =
            local.Length <= 2
                ? local[..1]
                : local[..2];


        return
            $"{visible}***@{domain}";
    }


    private static string? Clean(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
            value)
            ? null
            : value.Trim();
    }
}