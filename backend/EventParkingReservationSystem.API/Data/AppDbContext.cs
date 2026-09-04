using EventParkingReservationSystem.API.Models.Core;
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
    }
}