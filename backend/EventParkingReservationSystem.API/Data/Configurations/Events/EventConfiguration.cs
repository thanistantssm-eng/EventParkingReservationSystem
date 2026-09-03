using EventParkingReservationSystem.API.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParkingReservationSystem.API.Data.Configurations.Events;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(e => e.EventType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.TicketPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.PosterUrl)
            .HasMaxLength(1000);

        builder.Property(e => e.EventQrCode)
            .HasMaxLength(300);

        builder.Property(e => e.RejectionReason)
            .HasMaxLength(1000);

        builder.HasOne(e => e.EventCategory)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.EventCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Organizer and Venue models are owned by Member 1.
        // Member 2 keeps the FK values but does not configure Core navigation properties.
        builder.HasIndex(e => e.OrganizerId);
        builder.HasIndex(e => e.VenueId);
        builder.HasIndex(e => e.EventCategoryId);
        builder.HasIndex(e => e.Status);

        builder.HasIndex(e => new
        {
            e.VenueId,
            e.StartDateTime,
            e.EndDateTime
        });

        builder.HasIndex(e => e.EventQrCode)
            .IsUnique()
            .HasFilter("[EventQrCode] IS NOT NULL");
    }
}
