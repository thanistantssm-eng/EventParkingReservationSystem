using System.Text;
using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Extensions;
using EventParkingReservationSystem.API.Interfaces.Repositories.Core;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Middleware;
using EventParkingReservationSystem.API.Repositories.Core;
using EventParkingReservationSystem.API.Services.Core;
using EventParkingReservationSystem.API.Services.Transactions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder =
    WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


// ============================================
// DATABASE
// ============================================

builder.Services.AddDbContext<AppDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration
                .GetConnectionString(
                    "DefaultConnection")));


// ============================================
// REPOSITORIES
// ============================================

builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();

builder.Services.AddScoped<
    IOrganizerRepository,
    OrganizerRepository>();

builder.Services.AddScoped<
    ILoginOtpRepository,
    LoginOtpRepository>();


// ============================================
// MEMBER 1 - CORE / AUTH
// ============================================

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IJwtTokenService,
    JwtTokenService>();

builder.Services.AddScoped<
    IEmailService,
    EmailService>();

builder.Services.AddScoped<
    IUserService,
    UserService>();

builder.Services.AddScoped<
    ICustomerService,
    CustomerService>();

builder.Services.AddScoped<
    IOrganizerService,
    OrganizerService>();

builder.Services.AddScoped<
    IPropertyService,
    PropertyService>();

builder.Services.AddScoped<
    IVenueService,
    VenueService>();


// ============================================
// MEMBER 2 - EVENT MANAGEMENT
// ============================================

builder.Services.AddEventServices();


// ============================================
// MEMBER 3 - BOOKING / PAYMENT
// ============================================

builder.Services.AddScoped<
    IBookingService,
    BookingService>();

builder.Services.AddScoped<
    IPaymentService,
    PaymentService>();

builder.Services.AddScoped<
    IOtpService,
    OtpService>();

builder.Services.AddScoped<
    IQrCodeService,
    QrCodeService>();
// Core notifications
builder.Services.AddScoped<
    EventParkingReservationSystem.API.Interfaces.Services.Core.INotificationService,
    EventParkingReservationSystem.API.Services.Core.NotificationService>();

// Booking / transaction notifications
builder.Services.AddScoped<
    EventParkingReservationSystem.API.Interfaces.Transactions.INotificationService,
    EventParkingReservationSystem.API.Services.Transactions.NotificationService>();

builder.Services.AddScoped<
    IReportService,
    ReportService>();


// ============================================
// JWT AUTHENTICATION
// ============================================

var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Jwt:Key configuration missing.");

var issuer =
    builder.Configuration["Jwt:Issuer"];

var audience =
    builder.Configuration["Jwt:Audience"];

builder.Services
    .AddAuthentication(
        options =>
        {
            options.DefaultAuthenticateScheme =
                JwtBearerDefaults.AuthenticationScheme;

            options.DefaultChallengeScheme =
                JwtBearerDefaults.AuthenticationScheme;
        })
    .AddJwtBearer(
        options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = issuer,
                    ValidAudience = audience,

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtKey)),

                    ClockSkew = TimeSpan.Zero
                };
        });

builder.Services.AddAuthorization();


// ============================================
// CORS
// ============================================

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "FrontendPolicy",
            policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
    });


// ============================================
// SWAGGER
// ============================================

builder.Services.AddSwaggerGen(
    options =>
    {
        options.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Title =
                    "EventParkingReservationSystem.API",

                Version = "v1"
            });

        options.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Name = "Authorization",

                Type =
                    SecuritySchemeType.Http,

                Scheme = "bearer",

                BearerFormat = "JWT",

                In =
                    ParameterLocation.Header,

                Description =
                    "Enter JWT token"
            });

        options.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference =
                            new OpenApiReference
                            {
                                Type =
                                    ReferenceType.SecurityScheme,

                                Id = "Bearer"
                            }
                    },

                    Array.Empty<string>()
                }
            });
    });


var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();


// ============================================
// PIPELINE
// ============================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapGet(
    "/api/health",
    () =>
        Results.Ok(
            new
            {
                status = "ok",
                service =
                    "Event Parking Reservation System API"
            }));

app.Run();

public partial class Program;