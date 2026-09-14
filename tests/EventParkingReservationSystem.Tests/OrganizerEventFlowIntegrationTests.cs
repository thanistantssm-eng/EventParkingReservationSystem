using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Events;
using EventParkingReservationSystem.API.Models.Core;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Repositories.Events.Implementations;
using EventParkingReservationSystem.API.Services.Core;
using EventParkingReservationSystem.API.Services.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EventParkingReservationSystem.Tests;

public sealed class OrganizerEventFlowIntegrationTests
{
    [Fact]
    public async Task AuthenticatedOrganizer_CanCreateSubmitApprovePublish_AndCustomerSeesOnlyPublished()
    {
        var databaseName = $"EventoraOrganizerFlow_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True")
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();

        try
        {
            var admin = new User
            {
                Username = "admin",
                Email = "admin@example.test",
                PasswordHash = "test",
                Role = UserRole.Admin
            };
            var organizerUser = new User
            {
                Username = "organizer",
                Email = "organizer@example.test",
                PasswordHash = "test",
                Role = UserRole.Organizer,
                Organizer = new Organizer
                {
                    OrganizationName = "Integration Events",
                    // Account verification is an admin-facing profile state. It
                    // must not make an existing active organizer look missing
                    // when the organizer creates a Draft event.
                    IsVerified = false
                }
            };
            var category = new EventCategory
            {
                Name = "Integration Category",
                IsActive = true
            };

            db.Users.AddRange(admin, organizerUser);
            db.EventCategories.Add(category);
            await db.SaveChangesAsync();

            // Reproduce a token issued with a stale organizerId that contains
            // User.Id instead of Organizer.Id. Authentication normalization
            // must replace it from the database relationship.
            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, organizerUser.Id.ToString()),
                    new Claim(ClaimTypes.Role, UserRole.Organizer.ToString()),
                    new Claim("organizerId", organizerUser.Id.ToString())
                },
                "Test");
            var principal = new ClaimsPrincipal(identity);
            var claimsTransformation = new OrganizerClaimsTransformation(
                db,
                NullLogger<OrganizerClaimsTransformation>.Instance);

            await claimsTransformation.TransformAsync(principal);

            var organizerId = int.Parse(principal.FindFirstValue("organizerId")!);
            Assert.Equal(organizerUser.Organizer!.Id, organizerId);
            Assert.NotEqual(organizerUser.Id, organizerId);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "integration-test-signing-key-32-characters-long",
                    ["Jwt:Issuer"] = "integration-tests",
                    ["Jwt:Audience"] = "integration-tests",
                    ["Jwt:ExpiryMinutes"] = "10"
                })
                .Build();
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(
                new JwtTokenService(configuration).GenerateToken(organizerUser));
            Assert.Contains(jwt.Claims, x => x.Type == "userId" && x.Value == organizerUser.Id.ToString());
            Assert.Contains(jwt.Claims, x => x.Type == "organizerId" && x.Value == organizerId.ToString());

            var eventRepository = new EventRepository(db);
            var categoryRepository = new EventCategoryRepository(db);
            var ticketRepository = new TicketRepository(db);
            var seatRepository = new SeatRepository(db);
            var eventService = new EventService(
                eventRepository,
                categoryRepository,
                ticketRepository,
                seatRepository,
                db,
                referenceReadService: new EventReferenceReadService(db));

            var created = await eventService.CreateAsync(
                NewEvent(category.Id, "Organizer Published Event"),
                organizerUser.Id,
                organizerId,
                UserRole.Organizer.ToString());

            Assert.Equal("Draft", created.Status);
            Assert.Equal(organizerId, created.OrganizerId);
            Assert.False(organizerUser.Organizer.IsVerified);

            db.TicketTypes.Add(new TicketType
            {
                EventId = created.Id,
                Name = "General",
                Price = 1500,
                Quantity = 100,
                IsActive = true
            });
            await db.SaveChangesAsync();

            var approvalService = new ApprovalService(
                new ApprovalRepository(db),
                eventRepository,
                ticketRepository,
                seatRepository,
                db);
            var submitted = await approvalService.SubmitAsync(
                created.Id,
                new SubmitApprovalDto(),
                organizerUser.Id,
                organizerId,
                UserRole.Organizer.ToString());
            Assert.Equal("Pending", submitted.Status);

            var approved = await approvalService.ApproveAsync(
                submitted.Id,
                new ReviewApprovalDto(),
                admin.Id,
                UserRole.Admin.ToString());
            Assert.Equal("Approved", approved.Status);

            var published = await eventService.PublishAsync(
                created.Id,
                admin.Id,
                UserRole.Admin.ToString());
            Assert.Equal("Published", published.Status);

            var draft = await eventService.CreateAsync(
                NewEvent(category.Id, "Organizer Draft Event"),
                organizerUser.Id,
                organizerId,
                UserRole.Organizer.ToString());
            Assert.Equal("Draft", draft.Status);

            var customerEvents = await eventService.GetAllAsync(
                new EventQueryDto(),
                actorUserId: null,
                actorOrganizerId: null,
                actorRole: UserRole.Customer.ToString());

            Assert.Single(customerEvents);
            Assert.Equal(published.Id, customerEvents[0].Id);
        }
        finally
        {
            await db.Database.EnsureDeletedAsync();
        }
    }

    private static CreateEventDto NewEvent(int categoryId, string name) => new()
    {
        Name = name,
        Description = "Organizer event flow integration test",
        EventType = "NonSeatBased",
        VenueMode = "ExternalProperty",
        ExternalVenueName = "Test Hall",
        ExternalVenueAddress = "1 Test Road",
        EventCategoryId = categoryId,
        StartDateTime = DateTime.UtcNow.AddDays(5),
        EndDateTime = DateTime.UtcNow.AddDays(5).AddHours(2),
        TicketPrice = 1500
    };
}
