using EventParkingReservationSystem.API.Models.Core;
using EventParkingReservationSystem.API.Models.Transactions;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Data;

public class AppDbContext
    : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }


    public DbSet<User> Users =>
        Set<User>();

    public DbSet<Organizer> Organizers =>
        Set<Organizer>();

    public DbSet<LoginOtp> LoginOtps =>
        Set<LoginOtp>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<ParkingSlot> ParkingSlots => Set<ParkingSlot>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingTicket> BookingTickets => Set<BookingTicket>();
    public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();
    public DbSet<BookingParking> BookingParkings => Set<BookingParking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<QrCode> QrCodes => Set<QrCode>();
    public DbSet<Notification> Notifications => Set<Notification>();


    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(
            modelBuilder);


        // ============================
        // USER
        // ============================

        modelBuilder.Entity<User>(
            entity =>
            {
                entity.HasKey(x =>
                    x.Id);


                entity.HasIndex(x =>
                        x.Username)
                    .IsUnique();


                entity.HasIndex(x =>
                        x.Email)
                    .IsUnique();


                entity.Property(x =>
                        x.Username)
                    .IsRequired()
                    .HasMaxLength(50);


                entity.Property(x =>
                        x.Email)
                    .IsRequired()
                    .HasMaxLength(150);


                entity.Property(x =>
                        x.PasswordHash)
                    .IsRequired();


                entity.Property(x =>
                        x.Role)
                    .HasConversion<string>()
                    .HasMaxLength(20);


                entity.HasOne(x =>
                        x.Organizer)
                    .WithOne(x =>
                        x.User)
                    .HasForeignKey<Organizer>(
                        x => x.UserId)
                    .OnDelete(
                        DeleteBehavior.Cascade);
            });


        // ============================
        // ORGANIZER
        // ============================

        modelBuilder.Entity<Organizer>(
            entity =>
            {
                entity.HasKey(x =>
                    x.Id);


                entity.HasIndex(x =>
                        x.UserId)
                    .IsUnique();


                entity.Property(x =>
                        x.OrganizationName)
                    .IsRequired()
                    .HasMaxLength(150);


                entity.Property(x =>
                        x.PhoneNumber)
                    .HasMaxLength(20);


                entity.Property(x =>
                        x.Address)
                    .HasMaxLength(250);
            });


        // ============================
        // LOGIN OTP
        // ============================

        modelBuilder.Entity<LoginOtp>(
            entity =>
            {
                entity.HasKey(x =>
                    x.Id);


                entity.HasIndex(x =>
                        x.ChallengeId)
                    .IsUnique();


                entity.Property(x =>
                        x.OtpHash)
                    .IsRequired()
                    .HasMaxLength(128);


                entity.HasOne(x =>
                        x.User)
                    .WithMany(x =>
                        x.LoginOtps)
                    .HasForeignKey(x =>
                        x.UserId)
                    .OnDelete(
                        DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<Customer>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Event>().Property(x => x.TicketPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Event>().Property(x => x.ParkingFee).HasPrecision(18, 2);
        modelBuilder.Entity<Seat>().HasIndex(x => new { x.EventId, x.SeatNumber }).IsUnique();
        modelBuilder.Entity<ParkingSlot>().HasIndex(x => new { x.EventId, x.SlotNumber }).IsUnique();
        modelBuilder.Entity<Booking>().HasIndex(x => x.BookingNumber).IsUnique();
        modelBuilder.Entity<Booking>().Property(x => x.Status).HasConversion<string>();
        modelBuilder.Entity<Booking>().Property(x => x.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<BookingSeat>().HasIndex(x => x.SeatId).IsUnique();
        modelBuilder.Entity<BookingParking>().HasIndex(x => x.ParkingSlotId).IsUnique();
        modelBuilder.Entity<BookingParking>().HasIndex(x => x.BookingId).IsUnique();
        modelBuilder.Entity<BookingParking>().Property(x => x.Fee).HasPrecision(18, 2);
        modelBuilder.Entity<Payment>().HasIndex(x => x.BookingId).IsUnique();
        modelBuilder.Entity<Payment>().Property(x => x.Status).HasConversion<string>();
        modelBuilder.Entity<Payment>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<OtpVerification>().HasIndex(x => x.PaymentId).IsUnique();
        modelBuilder.Entity<QrCode>().HasIndex(x => x.BookingId).IsUnique();
        modelBuilder.Entity<QrCode>().HasIndex(x => x.Token).IsUnique();
        modelBuilder.Entity<Booking>().HasOne(x => x.Parking).WithOne(x => x.Booking)
            .HasForeignKey<BookingParking>(x => x.BookingId);
        modelBuilder.Entity<Booking>().HasOne(x => x.Payment).WithOne(x => x.Booking)
            .HasForeignKey<Payment>(x => x.BookingId);
        modelBuilder.Entity<Booking>().HasOne(x => x.QrCode).WithOne(x => x.Booking)
            .HasForeignKey<QrCode>(x => x.BookingId);
        modelBuilder.Entity<Booking>().HasQueryFilter(x => x.Status != BookingStatus.Cancelled);
    }
}
