using EventParkingReservationSystem.API.DTOs.Customers;
using EventParkingReservationSystem.API.Interfaces.Repositories.Core;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.Services.Core;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    // =========================================================
    // CUSTOMER - GET OWN PROFILE
    // =========================================================

    public async Task<CustomerProfileDto?> GetMyProfileAsync(
        int userId)
    {
        var customer =
            await _customerRepository.GetByUserIdAsync(userId);

        if (customer is null)
        {
            return null;
        }

        return MapToCustomerProfileDto(customer);
    }

    // =========================================================
    // CUSTOMER - UPDATE OWN PROFILE
    // =========================================================

    public async Task<CustomerProfileDto?> UpdateMyProfileAsync(
        int userId,
        UpdateCustomerProfileDto request)
    {
        var customer =
            await _customerRepository.GetByUserIdAsync(userId);

        if (customer is null)
        {
            return null;
        }

        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Customer name is required.");
        }

        customer.Name = name;

        customer.Phone =
            Clean(request.Phone);

        customer.UpdatedAt =
            DateTime.UtcNow;

        // Keep user audit field updated too
        customer.User.UpdatedAt =
            DateTime.UtcNow;

        await _customerRepository.SaveChangesAsync();

        return MapToCustomerProfileDto(customer);
    }

    // =========================================================
    // ADMIN - GET ALL CUSTOMERS
    // =========================================================

    public async Task<IReadOnlyList<AdminCustomerDto>>
        GetAllForAdminAsync(
            string? search = null,
            bool? isActive = null)
    {
        var customers =
            await _customerRepository.GetAllAsync(search);

        IEnumerable<Customer> query = customers;

        if (isActive.HasValue)
        {
            query = query.Where(x =>
                x.User.IsActive == isActive.Value);
        }

        return query
            .Select(MapToAdminCustomerDto)
            .ToList();
    }

    // =========================================================
    // ADMIN - GET CUSTOMER BY ID
    // =========================================================

    public async Task<AdminCustomerDto?> GetByIdForAdminAsync(
        int id)
    {
        var customer =
            await _customerRepository.GetByIdAsync(id);

        if (customer is null)
        {
            return null;
        }

        return MapToAdminCustomerDto(customer);
    }

    // =========================================================
    // ADMIN - ACTIVATE / DEACTIVATE CUSTOMER
    // =========================================================

    public async Task<AdminCustomerDto?> SetStatusAsync(
        int id,
        bool isActive)
    {
        var customer =
            await _customerRepository.GetByIdAsync(id);

        if (customer is null)
        {
            return null;
        }

        customer.User.IsActive =
            isActive;

        customer.User.UpdatedAt =
            DateTime.UtcNow;

        customer.UpdatedAt =
            DateTime.UtcNow;

        await _customerRepository.SaveChangesAsync();

        return MapToAdminCustomerDto(customer);
    }

    // =========================================================
    // CUSTOMER PROFILE DTO MAPPING
    // =========================================================

    private static CustomerProfileDto
        MapToCustomerProfileDto(
            Customer customer)
    {
        return new CustomerProfileDto
        {
            Id = customer.Id,

            UserId = customer.UserId,

            Username =
                customer.User.Username,

            Name =
                customer.Name,

            Email =
                customer.Email,

            Phone =
                customer.Phone,

            IsActive =
                customer.User.IsActive,

            CreatedAt =
                customer.CreatedAt,

            UpdatedAt =
                customer.UpdatedAt
        };
    }

    // =========================================================
    // ADMIN CUSTOMER DTO MAPPING
    // =========================================================

    private static AdminCustomerDto
        MapToAdminCustomerDto(
            Customer customer)
    {
        return new AdminCustomerDto
        {
            Id = customer.Id,

            UserId = customer.UserId,

            Username =
                customer.User.Username,

            Name =
                customer.Name,

            Email =
                customer.Email,

            Phone =
                customer.Phone,

            IsActive =
                customer.User.IsActive,

            CreatedAt =
                customer.CreatedAt,

            UpdatedAt =
                customer.UpdatedAt
        };
    }

    // =========================================================
    // CLEAN OPTIONAL STRING
    // =========================================================

    private static string? Clean(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}