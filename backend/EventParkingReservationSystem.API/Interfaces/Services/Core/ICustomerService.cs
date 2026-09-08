using EventParkingReservationSystem.API.DTOs.Customers;

namespace EventParkingReservationSystem.API.Interfaces.Services.Core;

public interface ICustomerService
{
    Task<CustomerProfileDto?> GetMyProfileAsync(
        int userId);

    Task<CustomerProfileDto?> UpdateMyProfileAsync(
        int userId,
        UpdateCustomerProfileDto request);
}