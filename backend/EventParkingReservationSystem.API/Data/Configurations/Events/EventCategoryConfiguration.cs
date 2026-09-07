using EventParkingReservationSystem.API.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParkingReservationSystem.API.Data.Configurations.Events;

public class EventCategoryConfiguration
    : IEntityTypeConfiguration<EventCategory>
{
    public void Configure(EntityTypeBuilder<EventCategory> builder)
    {
        builder.ToTable("EventCategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.IsActive)
            .IsRequired();

        // Category names should not be duplicated.
        builder.HasIndex(c => c.Name)
            .IsUnique();
    }
}