using EventParkingReservationSystem.API.DTOs.Customers;

namespace EventParkingReservationSystem.API.Interfaces.Services.Core;

public interface ICustomerService
{
    Task<CustomerProfileDto?> GetMyProfileAsync(
        int userId);

    Task<CustomerProfileDto?> UpdateMyProfileAsync(
        int userId,
        UpdateCustomerProfileDto request);


    Task<IReadOnlyList<AdminCustomerDto>>
        GetAllForAdminAsync(
            string? search,
            bool? isActive);

    Task<AdminCustomerDto?>
        GetByIdForAdminAsync(
            int customerId);

    Task<AdminCustomerDto?>
        SetStatusAsync(
            int customerId,
            bool isActive);
}