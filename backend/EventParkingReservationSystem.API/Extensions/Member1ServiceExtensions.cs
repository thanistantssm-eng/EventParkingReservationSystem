using System.Text;
using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Helpers;
using EventParkingReservationSystem.API.Interfaces.Repositories;
using EventParkingReservationSystem.API.Interfaces.Services;
using EventParkingReservationSystem.API.Models.Core;
using EventParkingReservationSystem.API.Repositories;
using EventParkingReservationSystem.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EventParkingReservationSystem.API.Extensions;

public static class Member1ServiceExtensions
{
    public static IServiceCollection AddMember1Services(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(
            options =>
                options.UseSqlServer(
                    configuration
                        .GetConnectionString(
                            "DefaultConnection")));

        services.Configure<JwtSettings>(
            configuration.GetSection("Jwt"));

        var jwt =
            configuration
                .GetSection("Jwt")
                .Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                "JWT configuration missing.");

        services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer =
                            jwt.Issuer,

                        ValidAudience =
                            jwt.Audience,

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8
                                    .GetBytes(
                                        jwt.Key)),

                        ClockSkew =
                            TimeSpan.Zero
                    };
            });

        services.AddAuthorization();

        services.AddCors(options =>
        {
            options.AddPolicy(
                "AngularClient",
                policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        services.AddScoped<
            IUserRepository,
            UserRepository>();

        services.AddScoped<
            IOrganizerRepository,
            OrganizerRepository>();

        services.AddScoped<
            IPropertyRepository,
            PropertyRepository>();

        services.AddScoped<
            IAuthService,
            AuthService>();

        services.AddScoped<
            IOrganizerService,
            OrganizerService>();

        services.AddScoped<
            IPropertyService,
            PropertyService>();

        services.AddScoped<
            IJwtTokenService,
            JwtTokenService>();

        services.AddScoped<
            IPasswordHasher<User>,
            PasswordHasher<User>>();

        return services;
    }
}