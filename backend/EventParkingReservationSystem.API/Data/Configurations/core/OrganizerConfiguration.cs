using EventParkingReservationSystem.API.Models.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParkingReservationSystem.API.Data.Configurations.Core;

public class OrganizerConfiguration :
    IEntityTypeConfiguration<Organizer>
{
    public void Configure(
        EntityTypeBuilder<Organizer> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganizationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Phone)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .HasConversion<string>();

        builder.HasOne(x => x.User)
            .WithOne(x => x.OrganizerProfile)
            .HasForeignKey<Organizer>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}