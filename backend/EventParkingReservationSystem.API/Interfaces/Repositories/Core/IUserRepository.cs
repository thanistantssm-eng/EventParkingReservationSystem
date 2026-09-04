using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.Interfaces.Repositories.Core;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();

    Task<User?> GetByIdAsync(int id);

    Task<User?> GetByIdentifierAsync(
        string identifier);

    Task<bool> UsernameExistsAsync(
        string username);

    Task<bool> EmailExistsAsync(
        string email);

    Task AddAsync(User user);

    Task SaveChangesAsync();
}