using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.Interfaces.Repositories.Core;

public interface IOrganizerRepository
{
    Task<List<Organizer>> GetAllAsync();

    Task<Organizer?> GetByIdAsync(int id);

    Task<Organizer?> GetByUserIdAsync(int userId);

    Task SaveChangesAsync();
}