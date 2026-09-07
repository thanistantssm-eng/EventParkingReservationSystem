using System.Text;
using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Interfaces.Repositories.Core;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Repositories.Core;
using EventParkingReservationSystem.API.Services.Core;
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
// SERVICES
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
    IOrganizerService,
    OrganizerService>();

builder.Services.AddScoped<
    IPropertyService,
    PropertyService>();

builder.Services.AddScoped<
    IVenueService,
    VenueService>();


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
                JwtBearerDefaults
                    .AuthenticationScheme;

            options.DefaultChallengeScheme =
                JwtBearerDefaults
                    .AuthenticationScheme;
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
                            Encoding.UTF8
                                .GetBytes(jwtKey)),

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
// SWAGGER + JWT AUTHORIZE BUTTON
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
                                    ReferenceType
                                        .SecurityScheme,

                                Id = "Bearer"
                            }
                    },
                    Array.Empty<string>()
                }
            });
    });


var app = builder.Build();


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