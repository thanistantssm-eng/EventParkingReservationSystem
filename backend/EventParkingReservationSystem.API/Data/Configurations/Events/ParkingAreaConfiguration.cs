using EventParkingReservationSystem.API.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParkingReservationSystem.API.Data.Configurations.Events;

public class ParkingAreaConfiguration : IEntityTypeConfiguration<ParkingArea>
{
    public void Configure(EntityTypeBuilder<ParkingArea> builder)
    {
        builder.ToTable("ParkingAreas");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Capacity)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.HasIndex(p => p.VenueId);

        builder.HasIndex(p => new
        {
            p.VenueId,
            p.Name
        })
        .IsUnique();
    }
}
