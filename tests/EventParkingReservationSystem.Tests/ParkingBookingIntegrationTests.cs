using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Transactions;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Interfaces.Transactions;
using EventParkingReservationSystem.API.Middleware;
using EventParkingReservationSystem.API.Models.Core;
using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Models.Transactions;
using EventParkingReservationSystem.API.Services.Transactions;
using EventParkingReservationSystem.API.Services.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EventParkingReservationSystem.Tests;

public sealed class ParkingBookingIntegrationTests
{
    [Fact]
    public async Task PaymentConfirmation_RacingExpiry_NeverConfirmsReleasedInventory()
    {
        await WithDatabaseAsync(async (db, options, fixture) =>
        {
            var booking = await new BookingService(db, new NoOpExpiryService()).CreateAsync(
                Request(fixture.CustomerAId, fixture.EventId, fixture.A03Id), CancellationToken.None);
            var payment = await new PaymentService(db, new NoOpExpiryService()).StartAsync(
                booking.Id, new PaymentRequestDto(), CancellationToken.None);
            db.OtpVerifications.Add(new OtpVerification { PaymentId = payment.Id,
                CodeHash = "test-authorized", ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
                VerifiedAtUtc = DateTime.UtcNow });
            var item = await db.Bookings.SingleAsync(x => x.Id == booking.Id);
            item.CreatedAtUtc = DateTime.UtcNow.AddMinutes(-20);
            await db.SaveChangesAsync();
            var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            async Task ConfirmAsync()
            {
                await using var context = new AppDbContext(options);
                await gate.Task;
                try { await new PaymentService(context, new NoOpExpiryService())
                    .CompleteAsync(payment.Id, CancellationToken.None); }
                catch (ConflictException) { }
            }
            async Task ExpireAsync()
            {
                await using var context = new AppDbContext(options);
                var config = new ConfigurationBuilder().AddInMemoryCollection(
                    new Dictionary<string, string?> { ["BookingHold:Minutes"] = "1" }).Build();
                await gate.Task;
                await new BookingExpiryService(context, config, new NoOpEmailService(),
                    NullLogger<BookingExpiryService>.Instance).ExpireStalePendingBookingsAsync(CancellationToken.None);
            }
            var confirm = ConfirmAsync();
            var expire = ExpireAsync();
            gate.SetResult();
            await Task.WhenAll(confirm, expire);
            var actualBooking = await db.Bookings.AsNoTracking().SingleAsync(x => x.Id == booking.Id);
            var actualPayment = await db.Payments.AsNoTracking().SingleAsync(x => x.Id == payment.Id);
            var parkingExists = await db.BookingParkings.AnyAsync(x => x.BookingId == booking.Id);
            var qrExists = await db.QrCodes.AnyAsync(x => x.BookingId == booking.Id);
            if (actualBooking.Status == BookingStatus.Confirmed)
            {
                Assert.Equal(PaymentStatus.Completed, actualPayment.Status);
                Assert.True(parkingExists);
                Assert.True(qrExists);
            }
            else
            {
                Assert.Equal(BookingStatus.Cancelled, actualBooking.Status);
                Assert.Equal(PaymentStatus.Failed, actualPayment.Status);
                Assert.False(parkingExists);
                Assert.False(qrExists);
            }
        });
    }

    [Fact]
    public async Task SharedParkingArea_ListingMatchesGlobalSlotReservationConstraint()
    {
        await WithDatabaseAsync(async (db, _, fixture) =>
        {
            var service = new BookingService(db, new NoOpExpiryService());
            await service.CreateAsync(Request(fixture.CustomerAId, fixture.EventId, fixture.A03Id), CancellationToken.None);
            var original = await db.Events.AsNoTracking().SingleAsync(x => x.Id == fixture.EventId);
            var second = new EventParkingReservationSystem.API.Models.Events.Event
            {
                Name = "Second event", Description = "Shared parking area",
                EventType = original.EventType, VenueId = original.VenueId,
                EventCategoryId = original.EventCategoryId, CreatedByUserId = original.CreatedByUserId,
                StartDateTime = original.StartDateTime, EndDateTime = original.EndDateTime,
                Status = EventStatus.Published
            };
            db.Events.Add(second);
            await db.SaveChangesAsync();
            var areaId = await db.ParkingSlots.Where(x => x.Id == fixture.A03Id)
                .Select(x => x.ParkingAreaId).SingleAsync();
            db.EventParkingAllocations.Add(new EventParkingAllocation
            {
                EventId = second.Id, ParkingAreaId = areaId, AllocatedSlotCount = 10,
                ParkingFee = 500, IsActive = true
            });
            await db.SaveChangesAsync();
            var occupied = await new EventBookingReadService(db)
                .GetOccupiedParkingSlotIdsAsync(second.Id);
            Assert.Contains(fixture.A03Id, occupied);
        });
    }

    [Fact]
    public async Task CheckoutRetry_OtpConfirmation_AndRefund_PreserveOneReservation()
    {
        await WithDatabaseAsync(async (db, _, fixture) =>
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string?> { ["BookingHold:Minutes"] = "15" }).Build();
            var email = new CapturingEmailService();
            var expiry = new BookingExpiryService(db, config, email,
                NullLogger<BookingExpiryService>.Instance);
            var service = new BookingService(db, expiry);
            var request = Request(fixture.CustomerAId, fixture.EventId, null);
            request.RequestId = Guid.NewGuid();
            var booking = await service.CreateAsync(request, CancellationToken.None);
            var retry = await service.CreateAsync(request, CancellationToken.None);
            Assert.Equal(booking.Id, retry.Id);

            var separateOrder = Request(fixture.CustomerAId, fixture.EventId, null);
            separateOrder.RequestId = Guid.NewGuid();
            Assert.NotEqual(booking.Id,
                (await service.CreateAsync(separateOrder, CancellationToken.None)).Id);

            request.Quantity = 2;
            await Assert.ThrowsAsync<ConflictException>(() =>
                service.CreateAsync(request, CancellationToken.None));
            request.Quantity = 1;

            var payments = new PaymentService(db, expiry);
            var payment = await payments.StartAsync(booking.Id,
                new PaymentRequestDto(), CancellationToken.None);
            Assert.Equal(payment.Id, (await payments.StartAsync(booking.Id,
                new PaymentRequestDto(), CancellationToken.None)).Id);
            await Assert.ThrowsAsync<ValidationException>(() =>
                payments.CompleteAsync(payment.Id, CancellationToken.None));

            var otp = new OtpService(db, payments, expiry, email,
                new TestEnvironment(), config);
            await otp.IssueAsync(payment.Id, fixture.CustomerAId, CancellationToken.None);
            var wrongCode = email.Code == "000000" ? "000001" : "000000";
            await Assert.ThrowsAsync<ValidationException>(() => otp.VerifyAsync(
                new OtpVerifyDto { PaymentId = payment.Id, Code = wrongCode },
                fixture.CustomerAId, CancellationToken.None));
            Assert.Equal(1, await db.OtpVerifications.AsNoTracking()
                .Where(x => x.PaymentId == payment.Id).Select(x => x.FailedAttempts).SingleAsync());

            await otp.IssueAsync(payment.Id, fixture.CustomerAId, CancellationToken.None);
            Assert.Equal(1, await db.OtpVerifications.CountAsync(x => x.PaymentId == payment.Id));
            var verified = await otp.VerifyAsync(
                new OtpVerifyDto { PaymentId = payment.Id, Code = email.Code },
                fixture.CustomerAId, CancellationToken.None);
            Assert.Equal("Completed", verified.Status);
            Assert.Equal("Completed", (await otp.VerifyAsync(
                new OtpVerifyDto { PaymentId = payment.Id, Code = email.Code },
                fixture.CustomerAId, CancellationToken.None)).Status);
            Assert.Equal("Confirmed", (await service.CreateAsync(request, CancellationToken.None)).Status);
            Assert.Equal(1, await db.QrCodes.CountAsync(x => x.BookingId == booking.Id));
            await Assert.ThrowsAsync<ConflictException>(() =>
                service.CancelAsync(booking.Id, fixture.CustomerAId, CancellationToken.None));
            Assert.Equal("Refunded", (await payments.RefundAsync(payment.Id, CancellationToken.None)).Status);
            Assert.Equal("Refunded", (await payments.RefundAsync(payment.Id, CancellationToken.None)).Status);
            Assert.Equal("Cancelled", (await service.GetAsync(booking.Id, CancellationToken.None)).Status);
            Assert.False(await db.QrCodes.AnyAsync(x => x.BookingId == booking.Id));
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ConcurrentCustomers_SeatOrTicketCapacity_AllowsOnlyOneWinner(bool seatBased)
    {
        await WithDatabaseAsync(async (db, options, fixture) =>
        {
            var item = await db.Events.SingleAsync(x => x.Id == fixture.EventId);
            item.EventType = seatBased ? EventType.SeatBased : EventType.NonSeatBased;
            var ticket = await db.TicketTypes.SingleAsync(x => x.EventId == item.Id);
            ticket.Quantity = 1;
            var seat = new Seat { EventId = item.Id, TicketTypeId = ticket.Id,
                SeatNumber = "A01", IsActive = true, SetupStatus = SeatSetupStatus.Available };
            if (seatBased) db.Seats.Add(seat);
            await db.SaveChangesAsync();

            var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            async Task<BookingDto?> BookAsync(int customerId)
            {
                await using var context = new AppDbContext(options);
                var service = new BookingService(context, new NoOpExpiryService());
                var request = Request(customerId, item.Id, null);
                request.RequestId = Guid.NewGuid();
                if (seatBased) request.SeatIds = [seat.Id];
                await gate.Task;
                try { return await service.CreateAsync(request, CancellationToken.None); }
                catch (ConflictException) { return null; }
            }
            var first = BookAsync(fixture.CustomerAId);
            var second = BookAsync(fixture.CustomerBId);
            gate.SetResult();
            var results = await Task.WhenAll(first, second);
            Assert.Single(results, x => x is not null);
            Assert.Equal(1, await db.Bookings.CountAsync(x => x.EventId == item.Id));
            if (seatBased) Assert.Equal(1, await db.BookingSeats.CountAsync(x => x.SeatId == seat.Id));
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ConcurrentParkingRequests_RetriesResumeButOtherCustomersCannotSteal(bool sameCustomer)
    {
        await WithDatabaseAsync(async (db, options, fixture) =>
        {
            var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var requestId = Guid.NewGuid();
            async Task<BookingDto?> BookAsync(int customerId)
            {
                await using var context = new AppDbContext(options);
                var request = Request(customerId, fixture.EventId, fixture.A03Id);
                request.RequestId = requestId;
                await gate.Task;
                try { return await new BookingService(context, new NoOpExpiryService())
                    .CreateAsync(request, CancellationToken.None); }
                catch (ConflictException) { return null; }
            }
            var first = BookAsync(fixture.CustomerAId);
            var second = BookAsync(sameCustomer ? fixture.CustomerAId : fixture.CustomerBId);
            gate.SetResult();
            var results = await Task.WhenAll(first, second);
            Assert.Equal(sameCustomer ? 2 : 1, results.Count(x => x is not null));
            if (sameCustomer) Assert.Equal(results[0]!.Id, results[1]!.Id);
            Assert.Equal(1, await db.BookingParkings.CountAsync(x => x.ParkingSlotId == fixture.A03Id));
        });
    }

    private static async Task WithDatabaseAsync(
        Func<AppDbContext, DbContextOptions<AppDbContext>, Fixture, Task> test)
    {
        var databaseName = $"EventoraReservation_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True",
                sql => sql.EnableRetryOnFailure()).Options;
        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        try { await test(db, options, await SeedAsync(db)); }
        finally { await db.Database.EnsureDeletedAsync(); }
    }

    [Fact]
    public async Task ExactSlot_NoParking_Cancellation_AndExpiry_AreConcurrencySafe()
    {
        var databaseName = $"EventoraParkingTests_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True",
                sql => sql.EnableRetryOnFailure())
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

            var retriedCustomerABooking = await service.CreateAsync(Request(
                fixture.CustomerAId, fixture.EventId, fixture.A03Id), CancellationToken.None);
            Assert.Equal(customerABooking.Id, retriedCustomerABooking.Id);

            var payments = new PaymentService(db, noExpiry);
            var firstPayment = await payments.StartAsync(
                customerABooking.Id,
                new PaymentRequestDto { Method = "Card" },
                CancellationToken.None);
            var retriedPayment = await payments.StartAsync(
                customerABooking.Id,
                new PaymentRequestDto { Method = "Card" },
                CancellationToken.None);
            Assert.Equal(firstPayment.Id, retriedPayment.Id);

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

    private class NoOpEmailService : IEmailService
    {
        public Task SendLoginOtpAsync(string receiverEmail, string username, string otp) => Task.CompletedTask;
        public Task SendPasswordResetOtpAsync(string receiverEmail, string username, string otp) => Task.CompletedTask;
        public virtual Task SendPaymentOtpAsync(string receiverEmail, string customerName, string otp, string bookingNumber) => Task.CompletedTask;
        public Task SendBookingExpiredAsync(string receiverEmail, string customerName, string bookingNumber, int holdMinutes) => Task.CompletedTask;
        public Task SendEventReportAsync(string receiverEmail, string organizerName, string eventName, string reportSummary) => Task.CompletedTask;
    }

    private sealed class CapturingEmailService : NoOpEmailService
    {
        public string Code { get; private set; } = string.Empty;
        public override Task SendPaymentOtpAsync(string receiverEmail, string customerName, string otp, string bookingNumber)
        {
            Code = otp;
            return Task.CompletedTask;
        }
    }

    private sealed class TestEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Production";
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
