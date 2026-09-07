using EventParkingReservationSystem.API.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Data;

public class AppDbContext : DbContext
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

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
    }
}