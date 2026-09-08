using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.Interfaces.Repositories.Core;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllAsync(string? search = null);

    Task<Customer?> GetByIdAsync(int id);

    Task<Customer?> GetByUserIdAsync(int userId);

    Task SaveChangesAsync();
}