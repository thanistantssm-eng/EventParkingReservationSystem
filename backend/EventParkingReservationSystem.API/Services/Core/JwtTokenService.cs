using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.IdentityModel.Tokens;

namespace EventParkingReservationSystem.API.Services.Core;

public class JwtTokenService
    : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(
        User user)
    {
        var jwtKey =
            _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT key is not configured.");

        var issuer =
            _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is not configured.");

        var audience =
            _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is not configured.");

        var claims =
            new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()
                ),

                new(
                    ClaimTypes.Name,
                    user.Username
                ),

                new(
                    ClaimTypes.Email,
                    user.Email
                ),

                new(
                    ClaimTypes.Role,
                    user.Role.ToString()
                )
            };

        var securityKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    jwtKey));

        var credentials =
            new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: GetExpirationTime(),
                signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    public DateTime GetExpirationTime()
    {
        var expiryMinutes =
            int.Parse(
                _configuration[
                    "Jwt:ExpiryMinutes"
                ] ?? "120");

        return DateTime.UtcNow
            .AddMinutes(expiryMinutes);
    }
}