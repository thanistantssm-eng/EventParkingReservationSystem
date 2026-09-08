using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Customers;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Middleware;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Services.Core;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(
        AppDbContext context)
    {
        _context = context;
    }


    // ============================================
    // GET LOGGED-IN CUSTOMER PROFILE
    // ============================================

    public async Task<CustomerProfileDto?>
        GetMyProfileAsync(
            int userId)
    {
        var user =
            await _context.Users
                .AsNoTracking()
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x =>
                    x.Id == userId &&
                    x.Role == UserRole.Customer);

        if (user?.Customer is null)
        {
            return null;
        }

        return Map(
            user,
            user.Customer);
    }


    // ============================================
    // UPDATE LOGGED-IN CUSTOMER PROFILE
    // ============================================

    public async Task<CustomerProfileDto?>
        UpdateMyProfileAsync(
            int userId,
            UpdateCustomerProfileDto request)
    {
        var user =
            await _context.Users
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x =>
                    x.Id == userId &&
                    x.Role == UserRole.Customer);


        if (user?.Customer is null)
        {
            return null;
        }


        var name =
            request.Name.Trim();

        var username =
            request.Username.Trim();

        var email =
            request.Email
                .Trim()
                .ToLowerInvariant();

        var phone =
            Clean(request.Phone);


        // ============================================
        // VALIDATION
        // ============================================

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(
                "Name is required.");
        }


        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ValidationException(
                "Username is required.");
        }


        // ============================================
        // CHECK DUPLICATE USERNAME
        // ============================================

        var usernameExists =
            await _context.Users
                .AnyAsync(x =>
                    x.Id != userId &&
                    x.Username.ToLower() ==
                    username.ToLower());


        if (usernameExists)
        {
            throw new ConflictException(
                "Username already exists.");
        }


        // ============================================
        // CHECK DUPLICATE EMAIL
        // ============================================

        var emailExistsInUsers =
            await _context.Users
                .AnyAsync(x =>
                    x.Id != userId &&
                    x.Email.ToLower() ==
                    email);


        var emailExistsInCustomers =
            await _context.Customers
                .AnyAsync(x =>
                    x.UserId != userId &&
                    x.Email.ToLower() ==
                    email);


        if (emailExistsInUsers ||
            emailExistsInCustomers)
        {
            throw new ConflictException(
                "Email already exists.");
        }


        // ============================================
        // UPDATE USER TABLE
        // ============================================

        user.Username =
            username;

        user.Email =
            email;

        user.UpdatedAt =
            DateTime.UtcNow;


        // ============================================
        // UPDATE CUSTOMER TABLE
        // ============================================

        user.Customer.Name =
            name;

        user.Customer.Email =
            email;

        user.Customer.Phone =
            phone;

        user.Customer.UpdatedAt =
            DateTime.UtcNow;


        await _context
            .SaveChangesAsync();


        return Map(
            user,
            user.Customer);
    }


    // ============================================
    // MAP
    // ============================================

    private static CustomerProfileDto Map(
        User user,
        Customer customer)
    {
        return new CustomerProfileDto
        {
            Id =
                customer.Id,

            UserId =
                user.Id,

            Name =
                customer.Name,

            Username =
                user.Username,

            Email =
                user.Email,

            Phone =
                customer.Phone,

            IsActive =
                user.IsActive,

            CreatedAt =
                customer.CreatedAt,

            UpdatedAt =
                customer.UpdatedAt
        };
    }


    private static string? Clean(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}