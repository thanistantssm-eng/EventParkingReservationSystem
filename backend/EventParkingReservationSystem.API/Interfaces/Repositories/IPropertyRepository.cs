using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.Interfaces.Repositories;

public interface IPropertyRepository
{
    Task<List<Property>> GetAllAsync();

    Task<Property?> GetByIdAsync(int id);

    Task AddAsync(Property property);

    Task SaveChangesAsync();
}