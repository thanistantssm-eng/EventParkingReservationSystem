using EventParkingReservationSystem.API.Models.Core;
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

    // ============================================
    // MEMBER 1 - CORE / AUTH
    // ============================================

    public DbSet<User> Users =>
        Set<User>();

    public DbSet<Organizer> Organizers =>
        Set<Organizer>();

    public DbSet<LoginOtp> LoginOtps =>
        Set<LoginOtp>();


    // ============================================
    // TRANSACTION / EVENT / BOOKING MODELS
    // ============================================

    public DbSet<Customer> Customers =>
        Set<Customer>();

    public DbSet<Event> Events =>
        Set<Event>();

    public DbSet<Seat> Seats =>
        Set<Seat>();

    public DbSet<ParkingSlot> ParkingSlots =>
        Set<ParkingSlot>();

    public DbSet<Booking> Bookings =>
        Set<Booking>();

    public DbSet<BookingTicket> BookingTickets =>
        Set<BookingTicket>();

    public DbSet<BookingSeat> BookingSeats =>
        Set<BookingSeat>();

    public DbSet<BookingParking> BookingParkings =>
        Set<BookingParking>();

    public DbSet<Payment> Payments =>
        Set<Payment>();

    public DbSet<OtpVerification> OtpVerifications =>
        Set<OtpVerification>();

    public DbSet<QrCode> QrCodes =>
        Set<QrCode>();

    public DbSet<Notification> Notifications =>
        Set<Notification>();


    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


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
                .HasForeignKey<Organizer>(
                    x => x.UserId)
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
        // EVENT
        // ============================================

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TicketPrice)
                .HasPrecision(18, 2);

            entity.Property(x => x.ParkingFee)
                .HasPrecision(18, 2);
        });


        // ============================================
        // SEAT
        // ============================================

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x =>
                new
                {
                    x.EventId,
                    x.SeatNumber
                })
                .IsUnique();
        });


        // ============================================
        // PARKING SLOT
        // ============================================

        modelBuilder.Entity<ParkingSlot>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x =>
                new
                {
                    x.EventId,
                    x.SlotNumber
                })
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
        });


        // ============================================
        // BOOKING TICKET
        // ============================================

        modelBuilder.Entity<BookingTicket>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UnitPrice)
                .HasPrecision(18, 2);
        });


        // ============================================
        // BOOKING SEAT
        // ============================================

        modelBuilder.Entity<BookingSeat>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.SeatId)
                .IsUnique();
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
        });


        // ============================================
        // BOOKING -> PARKING
        // ============================================

        modelBuilder.Entity<Booking>()
            .HasOne(x => x.Parking)
            .WithOne(x => x.Booking)
            .HasForeignKey<BookingParking>(
                x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);


        // ============================================
        // BOOKING -> PAYMENT
        // ============================================

        modelBuilder.Entity<Booking>()
            .HasOne(x => x.Payment)
            .WithOne(x => x.Booking)
            .HasForeignKey<Payment>(
                x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);


        // ============================================
        // BOOKING -> QR CODE
        // ============================================

        modelBuilder.Entity<Booking>()
            .HasOne(x => x.QrCode)
            .WithOne(x => x.Booking)
            .HasForeignKey<QrCode>(
                x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);



        var bookingEventForeignKey =
            modelBuilder.Entity<Booking>()
                .Metadata
                .GetForeignKeys()
                .FirstOrDefault(fk =>
                    fk.Properties.Any(
                        property =>
                            property.Name ==
                            nameof(Booking.EventId)));

        if (bookingEventForeignKey != null)
        {
            bookingEventForeignKey.DeleteBehavior =
                DeleteBehavior.NoAction;
        }


   
    }
}