using EventParkingReservationSystem.API.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParkingReservationSystem.API.Data.Configurations.Events;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("Seats");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SeatNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.RowLabel)
            .HasMaxLength(20);

        builder.Property(s => s.PriceOverride)
            .HasPrecision(18, 2);

        builder.Property(s => s.SetupStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .IsRequired();

        // Event -> Seats
        builder.HasOne(s => s.Event)
            .WithMany(e => e.Seats)
            .HasForeignKey(s => s.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // TicketType -> Seats
        builder.HasOne(s => s.TicketType)
            .WithMany(t => t.Seats)
            .HasForeignKey(s => s.TicketTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        /*
         * Same seat number cannot exist twice
         * inside the same event.
         *
         * Event 1 + A01 = valid
         * Event 1 + A01 again = rejected
         * Event 2 + A01 = valid
         */
        builder.HasIndex(s => new
        {
            s.EventId,
            s.SeatNumber
        })
        .IsUnique();

        builder.HasIndex(s => s.EventId);

        builder.HasIndex(s => s.TicketTypeId);
    }
}