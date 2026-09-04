using EventParkingReservationSystem.API.Interfaces.Events;
using EventParkingReservationSystem.API.Repositories.Events.Interfaces;
using EventParkingReservationSystem.API.Repositories.Events.Implementations;
using EventParkingReservationSystem.API.Services.Events;

namespace EventParkingReservationSystem.API.Extensions;

public static class EventServiceExtensions
{
    public static IServiceCollection AddEventServices(
        this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventCategoryRepository, EventCategoryRepository>();
        services.AddScoped<IApprovalRepository, ApprovalRepository>();
        services.AddScoped<ISeatRepository, SeatRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IParkingRepository, ParkingRepository>();

        // Services
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IEventCategoryService, EventCategoryService>();
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddScoped<ISeatService, SeatService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IParkingService, ParkingService>();

        // Cross-module adapters are intentionally NOT registered here:
        // - IEventReferenceReadService -> Member 1 (Venue/Organizer validation)
        // - IEventBookingReadService   -> Member 3 (active bookings + availability)
        // Their parameters are optional until the integration branches are merged.

        return services;
    }
}