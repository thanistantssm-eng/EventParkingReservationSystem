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

        // Member 3 booking integration.
        services.AddScoped<IEventBookingReadService, EventBookingReadService>();

        // IEventReferenceReadService is still pending because
        // the merged backend does not yet contain a canonical Venue model/service.
        // Organizer validation can be wired together with Venue integration.

        return services;
    }
}