using EventParkingReservationSystem.API.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParkingReservationSystem.API.Data.Configurations.Events;

public class ParkingSlotConfiguration
    : IEntityTypeConfiguration<ParkingSlot>
{
    public void Configure(EntityTypeBuilder<ParkingSlot> builder)
    {
        builder.ToTable("ParkingSlots");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.SlotNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.SlotType)
            .HasMaxLength(50);

        builder.Property(p => p.IsActive)
            .IsRequired();

        // ParkingArea -> ParkingSlots
        builder.HasOne(p => p.ParkingArea)
            .WithMany(a => a.Slots)
            .HasForeignKey(p => p.ParkingAreaId)
            .OnDelete(DeleteBehavior.Cascade);

        /*
         * Same parking area cannot contain
         * duplicate slot numbers.
         *
         * Area 1 -> P01
         * Area 1 -> P01 again = rejected
         * Area 2 -> P01 = valid
         */
        builder.HasIndex(p => new
        {
            p.ParkingAreaId,
            p.SlotNumber
        })
        .IsUnique();

        builder.HasIndex(p => p.ParkingAreaId);
    }
}