using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Middleware;
using EventParkingReservationSystem.API.Models.Core;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Models.Transactions;
using EventParkingReservationSystem.API.Services.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EventParkingReservationSystem.Tests;

public sealed class ParkingBookingIntegrationTests
{
    [Fact]
    public async Task ExactSlot_NoParking_Cancellation_AndExpiry_AreConcurrencySafe()
    {
        var databaseName = $"EventoraParkingTests_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True")
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();

        try
        {
            var fixture = await SeedAsync(db);
            var noExpiry = new NoOpExpiryService();
            var service = new BookingService(db, noExpiry);

            var customerABooking = await service.CreateAsync(Request(
                fixture.CustomerAId, fixture.EventId, fixture.A03Id), CancellationToken.None);
            Assert.Equal("A03", customerABooking.ParkingSlot);

            var conflict = await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(
                Request(fixture.CustomerBId, fixture.EventId, fixture.A03Id), CancellationToken.None));
            Assert.Contains("no longer available", conflict.Message, StringComparison.OrdinalIgnoreCase);

            var customerBBooking = await service.CreateAsync(Request(
                fixture.CustomerBId, fixture.EventId, fixture.A04Id), CancellationToken.None);
            Assert.Equal("A04", customerBBooking.ParkingSlot);

            var noParkingBooking = await service.CreateAsync(Request(
                fixture.CustomerCId, fixture.EventId, null), CancellationToken.None);
            Assert.Null(noParkingBooking.ParkingSlot);

            await service.CancelAsync(customerABooking.Id, fixture.CustomerAId, CancellationToken.None);
            var releasedAfterCancellation = await service.CreateAsync(Request(
                fixture.CustomerCId, fixture.EventId, fixture.A03Id), CancellationToken.None);
            Assert.Equal("A03", releasedAfterCancellation.ParkingSlot);

            var expiring = await db.Bookings.SingleAsync(x => x.Id == customerBBooking.Id);
            expiring.CreatedAtUtc = DateTime.UtcNow.AddMinutes(-5);
            await db.SaveChangesAsync();

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["BookingHold:Minutes"] = "1" })
                .Build();
            var expiry = new BookingExpiryService(
                db,
                config,
                new NoOpEmailService(),
                NullLogger<BookingExpiryService>.Instance);
            await expiry.ExpireStalePendingBookingsAsync(CancellationToken.None);

            var releasedAfterExpiry = await service.CreateAsync(Request(
                fixture.CustomerAId, fixture.EventId, fixture.A04Id), CancellationToken.None);
            Assert.Equal("A04", releasedAfterExpiry.ParkingSlot);
        }
        finally
        {
            await db.Database.EnsureDeletedAsync();
        }
    }

    private static CreateBookingDto Request(int customerId, int eventId, int? parkingSlotId) => new()
    {
        CustomerId = customerId,
        EventId = eventId,
        ParkingSlotId = parkingSlotId,
        TicketType = "General",
        Quantity = 1
    };

    private static async Task<Fixture> SeedAsync(AppDbContext db)
    {
        var property = new Property { Name = "Test Property", Address = "1 Test Road" };
        db.Properties.Add(property);
        await db.SaveChangesAsync();

        var venue = new Venue { PropertyId = property.Id, Name = "Test Arena", Capacity = 500, IsActive = true };
        db.Venues.Add(venue);
        var category = new EventCategory { Name = "Test Events", IsActive = true };
        db.EventCategories.Add(category);

        var users = new[]
        {
            new User { Username = "customer-a", Email = "a@example.test", PasswordHash = "test", Role = UserRole.Customer },
            new User { Username = "customer-b", Email = "b@example.test", PasswordHash = "test", Role = UserRole.Customer },
            new User { Username = "customer-c", Email = "c@example.test", PasswordHash = "test", Role = UserRole.Customer }
        };
        db.Users.AddRange(users);
        await db.SaveChangesAsync();

        var customers = users.Select((user, index) => new Customer
        {
            UserId = user.Id,
            Name = $"Customer {index + 1}",
            Email = user.Email
        }).ToArray();
        db.Customers.AddRange(customers);
        await db.SaveChangesAsync();

        var eventItem = new EventParkingReservationSystem.API.Models.Events.Event
        {
            Name = "Parking Integration Event",
            Description = "Relational parking reservation test",
            EventType = EventType.NonSeatBased,
            OrganizerId = null,
            VenueId = venue.Id,
            EventCategoryId = category.Id,
            StartDateTime = DateTime.UtcNow.AddDays(2),
            EndDateTime = DateTime.UtcNow.AddDays(2).AddHours(3),
            TicketPrice = 2500,
            Status = EventStatus.Published,
            CreatedByUserId = users[0].Id
        };
        db.Events.Add(eventItem);
        await db.SaveChangesAsync();

        db.TicketTypes.Add(new TicketType
        {
            EventId = eventItem.Id,
            Name = "General",
            Price = 2500,
            Quantity = 100,
            IsActive = true
        });
        var area = new ParkingArea { VenueId = venue.Id, Name = "Sky Arena Parking", Capacity = 10, IsActive = true };
        db.ParkingAreas.Add(area);
        await db.SaveChangesAsync();

        var slots = Enumerable.Range(1, 10).Select(number => new ParkingSlot
        {
            ParkingAreaId = area.Id,
            SlotNumber = $"A{number:00}",
            SlotType = "Standard",
            IsActive = true
        }).ToList();
        db.ParkingSlots.AddRange(slots);
        db.EventParkingAllocations.Add(new EventParkingAllocation
        {
            EventId = eventItem.Id,
            ParkingAreaId = area.Id,
            AllocatedSlotCount = 10,
            ParkingFee = 500,
            IsActive = true
        });
        await db.SaveChangesAsync();

        return new Fixture(
            eventItem.Id,
            customers[0].Id,
            customers[1].Id,
            customers[2].Id,
            slots.Single(x => x.SlotNumber == "A03").Id,
            slots.Single(x => x.SlotNumber == "A04").Id);
    }

    private sealed record Fixture(
        int EventId,
        int CustomerAId,
        int CustomerBId,
        int CustomerCId,
        int A03Id,
        int A04Id);

    private sealed class NoOpExpiryService : IBookingExpiryService
    {
        public Task ExpireStalePendingBookingsAsync(CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class NoOpEmailService : IEmailService
    {
        public Task SendLoginOtpAsync(string receiverEmail, string username, string otp) => Task.CompletedTask;
        public Task SendPasswordResetOtpAsync(string receiverEmail, string username, string otp) => Task.CompletedTask;
        public Task SendPaymentOtpAsync(string receiverEmail, string customerName, string otp, string bookingNumber) => Task.CompletedTask;
        public Task SendBookingExpiredAsync(string receiverEmail, string customerName, string bookingNumber, int holdMinutes) => Task.CompletedTask;
        public Task SendEventReportAsync(string receiverEmail, string organizerName, string eventName, string reportSummary) => Task.CompletedTask;
    }
}
