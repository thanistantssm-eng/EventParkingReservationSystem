using EventParkingReservationSystem.API.DTOs.Customers;
using EventParkingReservationSystem.API.Interfaces.Repositories.Core;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.Services.Core;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CustomerDto>> GetAllAsync(
        string? search = null)
    {
        var customers = await _repository.GetAllAsync(search);

        return customers.Select(Map).ToList();
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);

        return customer is null
            ? null
            : Map(customer);
    }

    public async Task<CustomerDto?> GetByUserIdAsync(int userId)
    {
        var customer = await _repository.GetByUserIdAsync(userId);

        return customer is null
            ? null
            : Map(customer);
    }

    public async Task<CustomerDto?> UpdateByUserIdAsync(
        int userId,
        UpdateCustomerDto request)
    {
        var customer = await _repository.GetByUserIdAsync(userId);

        if (customer is null)
        {
            return null;
        }

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Customer name is required.");
        }

        customer.Name = name;
        customer.Phone = Clean(request.Phone);
        customer.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        return Map(customer);
    }

    private static CustomerDto Map(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            UserId = customer.UserId,
            Username = customer.User.Username,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            IsActive = customer.User.IsActive,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}