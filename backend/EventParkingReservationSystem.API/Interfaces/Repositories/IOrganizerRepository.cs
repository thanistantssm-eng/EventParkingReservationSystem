using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.Interfaces.Repositories;

public interface IOrganizerRepository
{
    Task<List<Organizer>> GetAllAsync();

    Task<Organizer?> GetByIdAsync(int id);

    Task AddAsync(Organizer organizer);

    Task SaveChangesAsync();
}