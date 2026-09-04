using System.Security.Cryptography;
using System.Text;
using EventParkingReservationSystem.API.DTOs.Auth;
using EventParkingReservationSystem.API.Interfaces.Repositories.Core;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.AspNetCore.Identity;

namespace EventParkingReservationSystem.API.Services.Core;

public class AuthService
    : IAuthService
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

    private readonly PasswordHasher<User>
        _passwordHasher = new();


    public AuthService(
        IUserRepository userRepository,
        ILoginOtpRepository loginOtpRepository,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        IConfiguration configuration)
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
    }


    // =========================================
    // REGISTER
    // =========================================

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


        // Security:
        // Public registration cannot create Admin.
        if (role == UserRole.Admin)
        {
            throw new InvalidOperationException(
                "Admin registration is not allowed.");
        }


        if (role == UserRole.Organizer &&
            string.IsNullOrWhiteSpace(
                request.OrganizationName))
        {
            throw new InvalidOperationException(
                "Organization name is required for organizers.");
        }


        var user =
            new User
            {
                Username = username,
                Email = email,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };


        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                request.Password);


        if (role == UserRole.Organizer)
        {
            user.Organizer =
                new Organizer
                {
                    OrganizationName =
                        request.OrganizationName!
                            .Trim(),

                    PhoneNumber =
                        string.IsNullOrWhiteSpace(
                            request.PhoneNumber)
                            ? null
                            : request.PhoneNumber.Trim(),

                    Address =
                        string.IsNullOrWhiteSpace(
                            request.Address)
                            ? null
                            : request.Address.Trim(),

                    IsVerified = false,

                    CreatedAt =
                        DateTime.UtcNow
                };
        }


        await _userRepository
            .AddAsync(user);

        await _userRepository
            .SaveChangesAsync();


        return new RegisterResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }


    // =========================================
    // LOGIN STEP 1
    // USERNAME / EMAIL + PASSWORD
    // =========================================

    public async Task<LoginPendingResponseDto>
        LoginAsync(
            LoginRequestDto request)
    {
        var user =
            await _userRepository
                .GetByIdentifierAsync(
                    request.Identifier);


        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid username/email or password.");
        }


        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Your account is inactive.");
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


        // Password is correct.
        // IMPORTANT:
        // JWT IS NOT GENERATED HERE.

        await _loginOtpRepository
            .InvalidateActiveOtpsAsync(
                user.Id);


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

                FailedAttempts =
                    0,

                IsUsed =
                    false
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
                MaskEmail(user.Email),

            ExpiresAt =
                expiresAt
        };
    }


    // =========================================
    // LOGIN STEP 2
    // VERIFY OTP
    // =========================================

    public async Task<AuthResponseDto>
        VerifyLoginOtpAsync(
            VerifyLoginOtpRequestDto request)
    {
        var loginOtp =
            await _loginOtpRepository
                .GetByChallengeIdAsync(
                    request.ChallengeId);


        if (loginOtp is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid OTP session.");
        }


        if (loginOtp.IsUsed)
        {
            throw new UnauthorizedAccessException(
                "OTP is no longer valid.");
        }


        if (DateTime.UtcNow >
            loginOtp.ExpiresAt)
        {
            loginOtp.IsUsed =
                true;

            loginOtp.UsedAt =
                DateTime.UtcNow;

            await _loginOtpRepository
                .SaveChangesAsync();

            throw new UnauthorizedAccessException(
                "OTP has expired.");
        }


        if (loginOtp.FailedAttempts >= 5)
        {
            loginOtp.IsUsed =
                true;

            loginOtp.UsedAt =
                DateTime.UtcNow;

            await _loginOtpRepository
                .SaveChangesAsync();

            throw new UnauthorizedAccessException(
                "Too many invalid OTP attempts.");
        }


        if (!VerifyOtpHash(
                request.Otp,
                loginOtp.OtpHash))
        {
            loginOtp.FailedAttempts++;


            if (loginOtp.FailedAttempts >= 5)
            {
                loginOtp.IsUsed =
                    true;

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
                "Your account is inactive.");
        }


        loginOtp.IsUsed =
            true;

        loginOtp.UsedAt =
            DateTime.UtcNow;


        await _loginOtpRepository
            .SaveChangesAsync();


        // OTP SUCCESS.
        // NOW JWT IS GENERATED.

        var token =
            _jwtTokenService
                .GenerateToken(user);


        return new AuthResponseDto
        {
            Token =
                token,

            ExpiresAt =
                _jwtTokenService
                    .GetExpirationTime(),

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


    // =========================================
    // RESEND OTP
    // =========================================

    public async Task<LoginPendingResponseDto>
        ResendLoginOtpAsync(
            ResendLoginOtpRequestDto request)
    {
        var oldOtp =
            await _loginOtpRepository
                .GetByChallengeIdAsync(
                    request.ChallengeId);


        if (oldOtp is null)
        {
            throw new InvalidOperationException(
                "Login OTP session not found.");
        }


        if (oldOtp.IsUsed)
        {
            throw new InvalidOperationException(
                "OTP session is no longer valid.");
        }


        var nextAllowedTime =
            oldOtp.CreatedAt
                .AddSeconds(60);


        if (DateTime.UtcNow <
            nextAllowedTime)
        {
            var seconds =
                (int)Math.Ceiling(
                    (nextAllowedTime -
                     DateTime.UtcNow)
                    .TotalSeconds);

            throw new InvalidOperationException(
                $"Please wait {seconds} seconds before requesting another OTP.");
        }


        var user =
            oldOtp.User;


        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Your account is inactive.");
        }


        await _loginOtpRepository
            .InvalidateActiveOtpsAsync(
                user.Id);


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

                FailedAttempts =
                    0,

                IsUsed =
                    false
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
            RequiresOtp =
                true,

            ChallengeId =
                challengeId,

            MaskedEmail =
                MaskEmail(user.Email),

            ExpiresAt =
                expiresAt
        };
    }


    // =========================================
    // OTP HASH
    // =========================================

    private string HashOtp(
        string otp)
    {
        var pepper =
            _configuration["Otp:Pepper"]
            ?? throw new InvalidOperationException(
                "OTP pepper is not configured.");


        using var hmac =
            new HMACSHA256(
                Encoding.UTF8.GetBytes(
                    pepper));


        var hash =
            hmac.ComputeHash(
                Encoding.UTF8.GetBytes(
                    otp));


        return Convert.ToHexString(hash);
    }


    private bool VerifyOtpHash(
        string otp,
        string storedHash)
    {
        var generatedHash =
            HashOtp(otp);


        try
        {
            var generatedBytes =
                Convert.FromHexString(
                    generatedHash);

            var storedBytes =
                Convert.FromHexString(
                    storedHash);


            return CryptographicOperations
                .FixedTimeEquals(
                    generatedBytes,
                    storedBytes);
        }
        catch
        {
            return false;
        }
    }


    private static string MaskEmail(
        string email)
    {
        var atIndex =
            email.IndexOf('@');


        if (atIndex <= 0)
        {
            return "***";
        }


        var localPart =
            email[..atIndex];

        var domain =
            email[(atIndex + 1)..];


        string visiblePart;

        if (localPart.Length == 1)
        {
            visiblePart =
                localPart[..1];
        }
        else
        {
            visiblePart =
                localPart[..Math.Min(
                    2,
                    localPart.Length)];
        }


        return
            $"{visiblePart}***@{domain}";
    }
}