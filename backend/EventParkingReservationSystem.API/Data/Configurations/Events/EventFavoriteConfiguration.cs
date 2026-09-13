using EventParkingReservationSystem.API.Models.Events;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventParkingReservationSystem.API.Data.Configurations.Events;

public class EventFavoriteConfiguration : IEntityTypeConfiguration<EventFavorite>
{
    public void Configure(EntityTypeBuilder<EventFavorite> builder)
    {
        builder.ToTable("EventFavorites");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.CustomerId, x.EventId }).IsUnique();
        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Event)
            .WithMany()
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
