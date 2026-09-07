using EventParkingReservationSystem.API.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParkingReservationSystem.API.Data.Configurations.Events;

public class TicketTypeConfiguration
    : IEntityTypeConfiguration<TicketType>
{
    public void Configure(EntityTypeBuilder<TicketType> builder)
    {
        builder.ToTable("TicketTypes");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.Quantity)
            .IsRequired();

        builder.Property(t => t.IsActive)
            .IsRequired();

        // Event -> TicketTypes
        builder.HasOne(t => t.Event)
            .WithMany(e => e.TicketTypes)
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Same event cannot contain duplicate ticket type names.
        builder.HasIndex(t => new
        {
            t.EventId,
            t.Name
        })
        .IsUnique();

        builder.HasIndex(t => t.EventId);
    }
}