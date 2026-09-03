using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);

    Task<User?> GetByIdentifierAsync(string identifier);

    Task<bool> EmailExistsAsync(string email);

    Task<bool> UsernameExistsAsync(string username);

    Task AddAsync(User user);

    Task SaveChangesAsync();
}