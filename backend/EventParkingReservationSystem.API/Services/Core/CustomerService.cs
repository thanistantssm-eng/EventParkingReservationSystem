using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.DTOs.Customers;
using EventParkingReservationSystem.API.Interfaces.Services.Core;
using EventParkingReservationSystem.API.Middleware;
using EventParkingReservationSystem.API.Models.Core;
using EventParkingReservationSystem.API.Models.Transactions;
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
    // CUSTOMER - GET OWN PROFILE
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
    // CUSTOMER - UPDATE OWN PROFILE
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

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ValidationException(
                "Email is required.");
        }


        // ============================================
        // CHECK DUPLICATE USERNAME
        // ============================================

        var normalizedUsername =
            username.ToLower();

        var usernameExists =
            await _context.Users
                .AnyAsync(x =>
                    x.Id != userId &&
                    x.Username.ToLower() ==
                    normalizedUsername);

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


        await _context.SaveChangesAsync();


        return Map(
            user,
            user.Customer);
    }


    // ============================================
    // ADMIN - SEARCH / FILTER CUSTOMERS
    // ============================================

    public async Task<IReadOnlyList<AdminCustomerDto>>
        GetAllForAdminAsync(
            string? search,
            bool? isActive)
    {
        var query =
            BuildAdminCustomerQuery();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term =
                search.Trim();

            query =
                query.Where(x =>
                    x.Name.Contains(term) ||
                    x.Email.Contains(term) ||
                    x.Username.Contains(term));
        }

        if (isActive.HasValue)
        {
            query =
                query.Where(x =>
                    x.IsActive ==
                    isActive.Value);
        }

        return await query
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }


    // ============================================
    // ADMIN - GET SINGLE CUSTOMER
    // ============================================

    public async Task<AdminCustomerDto?>
        GetByIdForAdminAsync(
            int customerId)
    {
        return await BuildAdminCustomerQuery()
            .SingleOrDefaultAsync(x =>
                x.Id == customerId);
    }


    // ============================================
    // ADMIN - ACTIVATE / DEACTIVATE CUSTOMER
    // ============================================

    public async Task<AdminCustomerDto?>
        SetStatusAsync(
            int customerId,
            bool isActive)
    {
        var customer =
            await _context.Customers
                .Include(x => x.User)
                .SingleOrDefaultAsync(x =>
                    x.Id == customerId);

        if (customer is null)
        {
            return null;
        }

        // Make sure this record is actually
        // a Customer user account.
        if (customer.User.Role !=
            UserRole.Customer)
        {
            throw new ValidationException(
                "The selected account is not a customer.");
        }

        customer.User.IsActive =
            isActive;

        customer.User.UpdatedAt =
            DateTime.UtcNow;

        customer.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdForAdminAsync(
            customerId);
    }


    // ============================================
    // ADMIN - CUSTOMER SUMMARY QUERY
    // ============================================

    private IQueryable<AdminCustomerDto>
        BuildAdminCustomerQuery()
    {
        return _context.Customers
            .AsNoTracking()
            .Select(customer =>
                new AdminCustomerDto
                {
                    Id =
                        customer.Id,

                    UserId =
                        customer.UserId,

                    Name =
                        customer.Name,

                    Username =
                        customer.User.Username,

                    Email =
                        customer.Email,

                    Phone =
                        customer.Phone,

                    IsActive =
                        customer.User.IsActive,


                    // =================================
                    // BOOKING SUMMARY
                    // =================================

                    TotalBookings =
                        _context.Bookings
                            .IgnoreQueryFilters()
                            .Count(booking =>
                                booking.CustomerId ==
                                customer.Id),

                    ConfirmedBookings =
                        _context.Bookings
                            .IgnoreQueryFilters()
                            .Count(booking =>
                                booking.CustomerId ==
                                    customer.Id &&
                                booking.Status ==
                                    BookingStatus.Confirmed),

                    PendingBookings =
                        _context.Bookings
                            .IgnoreQueryFilters()
                            .Count(booking =>
                                booking.CustomerId ==
                                    customer.Id &&
                                booking.Status ==
                                    BookingStatus.PendingPayment),

                    CancelledBookings =
                        _context.Bookings
                            .IgnoreQueryFilters()
                            .Count(booking =>
                                booking.CustomerId ==
                                    customer.Id &&
                                booking.Status ==
                                    BookingStatus.Cancelled),


                    // =================================
                    // COMPLETED PAYMENT TOTAL
                    // =================================

                    TotalSpent =
                        _context.Payments
                            .IgnoreQueryFilters()
                            .Where(payment =>
                                payment.Booking.CustomerId ==
                                    customer.Id &&
                                payment.Status ==
                                    PaymentStatus.Completed)
                            .Sum(payment =>
                                (decimal?)payment.Amount)
                        ?? 0m,


                    // =================================
                    // LAST BOOKING
                    // =================================

                    LastBookingAtUtc =
                        _context.Bookings
                            .IgnoreQueryFilters()
                            .Where(booking =>
                                booking.CustomerId ==
                                customer.Id)
                            .Max(booking =>
                                (DateTime?)
                                booking.CreatedAtUtc),


                    CreatedAt =
                        customer.CreatedAt,

                    UpdatedAt =
                        customer.UpdatedAt
                });
    }


    // ============================================
    // CUSTOMER PROFILE MAP
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


    // ============================================
    // CLEAN OPTIONAL STRING
    // ============================================

    private static string? Clean(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}