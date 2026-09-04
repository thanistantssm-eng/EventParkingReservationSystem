using EventParkingReservationSystem.API.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParkingReservationSystem.API.Data.Configurations.Events;

public class EventApprovalConfiguration
    : IEntityTypeConfiguration<EventApproval>
{
    public void Configure(EntityTypeBuilder<EventApproval> builder)
    {
        builder.ToTable("EventApprovals");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.OrganizerNotes)
            .HasMaxLength(1000);

        builder.Property(a => a.ReviewReason)
            .HasMaxLength(1000);

        // Event -> Approvals
        builder.HasOne(a => a.Event)
            .WithMany(e => e.Approvals)
            .HasForeignKey(a => a.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.EventId);

        builder.HasIndex(a => new
        {
            a.EventId,
            a.Status
        });

        builder.HasIndex(a => a.RequestedByUserId);

        builder.HasIndex(a => a.ReviewedByUserId);
    }
}