using EventParkingReservationSystem.API.Models.Core;
using EventParkingReservationSystem.API.Models.Events;
using EventEntity = EventParkingReservationSystem.API.Models.Events.Event;
using SeatEntity = EventParkingReservationSystem.API.Models.Events.Seat;
using ParkingSlotEntity = EventParkingReservationSystem.API.Models.Events.ParkingSlot;
using EventParkingReservationSystem.API.Models.Transactions;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

<<<<<<< HEAD
    public DbSet<User> Users =>
        Set<User>();
=======
    // ============================================
    // MEMBER 1 - CORE / AUTH
    // ============================================

    public DbSet<User> Users => Set<User>();
    public DbSet<Organizer> Organizers => Set<Organizer>();
    public DbSet<LoginOtp> LoginOtps => Set<LoginOtp>();
    public DbSet<Customer> Customers => Set<Customer>();
>>>>>>> origin/develop

    // ============================================
    // MEMBER 2 - EVENT MANAGEMENT
    // ============================================

<<<<<<< HEAD
    public DbSet<Customer> Customers =>
        Set<Customer>();

    public DbSet<LoginOtp> LoginOtps =>
        Set<LoginOtp>();

    public DbSet<Property> Properties =>
        Set<Property>();

    public DbSet<Venue> Venues =>
        Set<Venue>();

    public DbSet<UserNotification> UserNotifications =>
        Set<UserNotification>();
=======
    public DbSet<EventEntity> Events => Set<EventEntity>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<EventApproval> EventApprovals => Set<EventApproval>();
    public DbSet<TicketType> TicketTypes => Set<TicketType>();
    public DbSet<SeatEntity> Seats => Set<SeatEntity>();
    public DbSet<ParkingArea> ParkingAreas => Set<ParkingArea>();
    public DbSet<ParkingSlotEntity> ParkingSlots => Set<ParkingSlotEntity>();
    public DbSet<EventParkingAllocation> EventParkingAllocations =>
        Set<EventParkingAllocation>();

    // ============================================
    // MEMBER 3 - BOOKING / PAYMENT
    // ============================================

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingTicket> BookingTickets => Set<BookingTicket>();
    public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();
    public DbSet<BookingParking> BookingParkings => Set<BookingParking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<QrCode> QrCodes => Set<QrCode>();
    public DbSet<Notification> Notifications => Set<Notification>();
>>>>>>> origin/develop

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

<<<<<<< HEAD
        // USER
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Username)
                .IsUnique();

            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.Property(x => x.Username)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.Property(x => x.Role)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(x => x.Organizer)
                .WithOne(x => x.User)
                .HasForeignKey<Organizer>(
                    x => x.UserId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            entity.HasOne(x => x.Customer)
                .WithOne(x => x.User)
                .HasForeignKey<Customer>(
                    x => x.UserId)
                .OnDelete(
                    DeleteBehavior.Cascade);
        });

        // ORGANIZER
        modelBuilder.Entity<Organizer>(
            entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.UserId)
                    .IsUnique();

                entity.Property(
                        x => x.OrganizationName)
                    .IsRequired()
                    .HasMaxLength(150);
            });

        // CUSTOMER
        modelBuilder.Entity<Customer>(
            entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.UserId)
                    .IsUnique();

                entity.HasIndex(x => x.Email)
                    .IsUnique();

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.Email)
                    .IsRequired()
                    .HasMaxLength(150);
            });

        // LOGIN OTP
        modelBuilder.Entity<LoginOtp>(
            entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(
                        x => x.ChallengeId)
                    .IsUnique();

                entity.Property(x => x.OtpHash)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(x => x.User)
                    .WithMany(x => x.LoginOtps)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(
                        DeleteBehavior.Cascade);
            });

        // PROPERTY
        modelBuilder.Entity<Property>(
            entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.Name)
                    .IsUnique();

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.Address)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.HasMany(x => x.Venues)
                    .WithOne(x => x.Property)
                    .HasForeignKey(
                        x => x.PropertyId)
                    .OnDelete(
                        DeleteBehavior.Restrict);
            });

        // VENUE
        modelBuilder.Entity<Venue>(
            entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x =>
                        new
                        {
                            x.PropertyId,
                            x.Name
                        })
                    .IsUnique();

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);
            });

        // USER NOTIFICATION
        modelBuilder.Entity<UserNotification>(
            entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.UserId);

                entity.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Message)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(x => x.IsRead)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(
                        DeleteBehavior.Cascade);
            });
=======
        // Member 2 keeps each entity configuration in
        // Data/Configurations/Events/*.cs.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);

        // ============================================
        // USER
        // ============================================

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Username)
                .IsUnique();

            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.Property(x => x.Username)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.Property(x => x.Role)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(x => x.Organizer)
                .WithOne(x => x.User)
                .HasForeignKey<Organizer>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // ORGANIZER
        // ============================================

        modelBuilder.Entity<Organizer>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.UserId)
                .IsUnique();

            entity.Property(x => x.OrganizationName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(x => x.Address)
                .HasMaxLength(250);
        });

        // ============================================
        // LOGIN OTP
        // ============================================

        modelBuilder.Entity<LoginOtp>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.ChallengeId)
                .IsUnique();

            entity.Property(x => x.OtpHash)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasOne(x => x.User)
                .WithMany(x => x.LoginOtps)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // CUSTOMER
        // ============================================

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Email)
                .IsUnique();
        });

        // ============================================
        // BOOKING
        // ============================================

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.BookingNumber)
                .IsUnique();

            entity.Property(x => x.Status)
                .HasConversion<string>();

            entity.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            // Booking now references Member 2's canonical Event entity.
            // NoAction prevents deleting an Event through a booking cascade.
            entity.HasOne(x => x.Event)
                .WithMany()
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ============================================
        // BOOKING TICKET
        // ============================================

        modelBuilder.Entity<BookingTicket>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            entity.HasOne(x => x.Booking)
                .WithMany(x => x.Tickets)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // BOOKING SEAT
        // ============================================

        modelBuilder.Entity<BookingSeat>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.SeatId)
                .IsUnique();

            entity.HasOne(x => x.Booking)
                .WithMany(x => x.Seats)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Seat)
                .WithMany()
                .HasForeignKey(x => x.SeatId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============================================
        // BOOKING PARKING
        // ============================================

        modelBuilder.Entity<BookingParking>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.ParkingSlotId)
                .IsUnique();

            entity.HasIndex(x => x.BookingId)
                .IsUnique();

            entity.Property(x => x.Fee)
                .HasPrecision(18, 2);

            entity.HasOne(x => x.Booking)
                .WithOne(x => x.Parking)
                .HasForeignKey<BookingParking>(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ParkingSlot)
                .WithMany()
                .HasForeignKey(x => x.ParkingSlotId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============================================
        // PAYMENT
        // ============================================

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.BookingId)
                .IsUnique();

            entity.Property(x => x.Status)
                .HasConversion<string>();

            entity.Property(x => x.Amount)
                .HasPrecision(18, 2);

            entity.HasOne(x => x.Booking)
                .WithOne(x => x.Payment)
                .HasForeignKey<Payment>(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // PAYMENT OTP
        // ============================================

        modelBuilder.Entity<OtpVerification>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.PaymentId)
                .IsUnique();
        });

        // ============================================
        // QR CODE
        // ============================================

        modelBuilder.Entity<QrCode>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.BookingId)
                .IsUnique();

            entity.HasIndex(x => x.Token)
                .IsUnique();

            entity.HasOne(x => x.Booking)
                .WithOne(x => x.QrCode)
                .HasForeignKey<QrCode>(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });
>>>>>>> origin/develop
    }
}
