using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventParkingReservationSystem.API.Interfaces.Services;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventParkingReservationSystem.API.Helpers;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(
        IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public TokenResult CreateToken(User user)
    {
        var expiresAt = DateTime.UtcNow
            .AddMinutes(_settings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.Name,
                user.Username),

            new(
                ClaimTypes.Email,
                user.Email),

            new(
                ClaimTypes.Role,
                user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.Key));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        return new TokenResult(
            tokenString,
            expiresAt);
    }
}