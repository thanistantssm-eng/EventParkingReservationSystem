using System.Text;
using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Extensions;
using EventParkingReservationSystem.API.Interfaces.Events;
using EventParkingReservationSystem.API.Interfaces.Repositories.Core;
using EventParkingReservationSystem.API.Interfaces.Services.Dashboards;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Middleware;
using EventParkingReservationSystem.API.Repositories.Core;
using EventParkingReservationSystem.API.Services.Core;
using EventParkingReservationSystem.API.Services.Dashboards;
using EventParkingReservationSystem.API.Services.Transactions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// ============================================================
// NOTIFICATION SERVICE ALIASES
// Core + Transaction modules both have NotificationService
// ============================================================

using CoreNotificationContract =
    EventParkingReservationSystem.API.Interfaces.Services.Core.INotificationService;

using CoreNotificationService =
    EventParkingReservationSystem.API.Services.Core.NotificationService;

using TransactionNotificationContract =
    EventParkingReservationSystem.API.Interfaces.Transactions.INotificationService;

using TransactionNotificationService =
    EventParkingReservationSystem.API.Services.Transactions.NotificationService;


// ============================================================
// BUILDER
// ============================================================

var builder = WebApplication.CreateBuilder(args);


// ============================================================
// CONTROLLERS
// ============================================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();


// ============================================================
// DATABASE
// ============================================================

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"));
});


// ============================================================
// MEMBER 1 - REPOSITORIES
// ============================================================

builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();

builder.Services.AddScoped<
    IOrganizerRepository,
    OrganizerRepository>();

builder.Services.AddScoped<
    ILoginOtpRepository,
    LoginOtpRepository>();

builder.Services.AddScoped<
    ICustomerRepository,
    CustomerRepository>();


// ============================================================
// MEMBER 1 - SERVICES
// ============================================================

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
    IOrganizerService,
    OrganizerService>();

builder.Services.AddScoped<
    ICustomerService,
    CustomerService>();

builder.Services.AddScoped<
    IPropertyService,
    PropertyService>();

builder.Services.AddScoped<
    IVenueService,
    VenueService>();


// ============================================================
// CORE NOTIFICATION SERVICE
// ============================================================

builder.Services.AddScoped<
    CoreNotificationContract,
    CoreNotificationService>();


// ============================================================
// EVENT REFERENCE SERVICE
// ============================================================

builder.Services.AddScoped<
    IEventReferenceReadService,
    EventReferenceReadService>();


// ============================================================
// DASHBOARD SERVICE
// ============================================================

builder.Services.AddScoped<
    IDashboardService,
    DashboardService>();


// ============================================================
// MEMBER 2 - EVENT MANAGEMENT
// ============================================================

builder.Services.AddEventServices();


// ============================================================
// MEMBER 3 - BOOKING / PAYMENT / TRANSACTIONS
// ============================================================

builder.Services.AddScoped<
    IBookingExpiryService,
    BookingExpiryService>();

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

builder.Services.AddScoped<
    TransactionNotificationContract,
    TransactionNotificationService>();

builder.Services.AddScoped<
    IReportService,
    ReportService>();


// ============================================================
// JWT CONFIGURATION
// ============================================================

var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT key is missing.");

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer is missing.");

var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience is missing.");


// ============================================================
// AUTHENTICATION
// ============================================================

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,

                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtKey)),

                ClockSkew =
                    TimeSpan.Zero
            };
    });


// ============================================================
// AUTHORIZATION
// ============================================================

builder.Services.AddAuthorization();


// ============================================================
// CORS
// ============================================================

builder.Services.AddCors(options =>
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


// ============================================================
// SWAGGER
// ============================================================

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title =
                "EventParkingReservationSystem.API",

            Version =
                "v1"
        });

    // --------------------------------------------------------
    // JWT AUTHORIZE BUTTON
    // --------------------------------------------------------

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name =
                "Authorization",

            Type =
                SecuritySchemeType.Http,

            Scheme =
                "bearer",

            BearerFormat =
                "JWT",

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

                            Id =
                                "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});


// ============================================================
// BUILD APPLICATION
// ============================================================

var app =
    builder.Build();


// ============================================================
// GLOBAL EXCEPTION MIDDLEWARE
// ============================================================

app.UseMiddleware<ApiExceptionMiddleware>();


// ============================================================
// SWAGGER - DEVELOPMENT
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}


// ============================================================
// HTTP PIPELINE
// ============================================================

app.UseHttpsRedirection();

app.UseCors(
    "FrontendPolicy");


// IMPORTANT:
// Authentication must come before Authorization.

app.UseAuthentication();

app.UseAuthorization();


// ============================================================
// CONTROLLERS
// ============================================================

app.MapControllers();


// ============================================================
// HEALTH CHECK
// ============================================================

app.MapGet(
    "/api/health",
    () =>
        Results.Ok(
            new
            {
                status =
                    "ok",

                service =
                    "Event Parking Reservation System API"
            }));


// ============================================================
// RUN
// ============================================================

app.Run();


// ============================================================
// REQUIRED FOR INTEGRATION TESTING
// ============================================================

public partial class Program
{
}