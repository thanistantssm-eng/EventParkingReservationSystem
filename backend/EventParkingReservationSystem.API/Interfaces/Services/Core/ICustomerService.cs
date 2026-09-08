using EventParkingReservationSystem.API.DTOs.Customers;

namespace EventParkingReservationSystem.API.Interfaces.Services.Core;

public interface ICustomerService
{
    // Customer - own profile
    Task<CustomerProfileDto?> GetMyProfileAsync(
        int userId);

    Task<CustomerProfileDto?> UpdateMyProfileAsync(
        int userId,
        UpdateCustomerProfileDto request);

    // Admin - customer management
    Task<IReadOnlyList<AdminCustomerDto>> GetAllForAdminAsync(
        string? search = null,
        bool? isActive = null);

    Task<AdminCustomerDto?> GetByIdForAdminAsync(
        int id);

    Task<AdminCustomerDto?> SetStatusAsync(
        int id,
        bool isActive);
}