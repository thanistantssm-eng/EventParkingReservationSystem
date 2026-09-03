using EventParkingReservationSystem.API.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParkingReservationSystem.API.Data.Configurations.Events;

public class EventParkingAllocationConfiguration
    : IEntityTypeConfiguration<EventParkingAllocation>
{
    public void Configure(
        EntityTypeBuilder<EventParkingAllocation> builder)
    {
        builder.ToTable("EventParkingAllocations");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AllocatedSlotCount)
            .IsRequired();

        builder.Property(a => a.ParkingFee)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(a => a.IsActive)
            .IsRequired();

        // Event -> Parking Allocations
        builder.HasOne(a => a.Event)
            .WithMany(e => e.ParkingAllocations)
            .HasForeignKey(a => a.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // ParkingArea -> Event Allocations
        builder.HasOne(a => a.ParkingArea)
            .WithMany(p => p.EventAllocations)
            .HasForeignKey(a => a.ParkingAreaId)
            .OnDelete(DeleteBehavior.Restrict);

        /*
         * Same parking area can be allocated only once
         * to the same event.
         */
        builder.HasIndex(a => new
        {
            a.EventId,
            a.ParkingAreaId
        })
        .IsUnique();

        builder.HasIndex(a => a.EventId);

        builder.HasIndex(a => a.ParkingAreaId);
    }
}