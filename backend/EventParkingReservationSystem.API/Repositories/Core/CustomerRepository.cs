using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Interfaces.Repositories.Core;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Repositories.Core;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Customer>> GetAllAsync(
        string? search = null)
    {
        var query = _context.Customers
            .Include(x => x.User)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLower();

            query = query.Where(x =>
                x.Name.ToLower().Contains(value) ||
                x.Email.ToLower().Contains(value) ||
                x.User.Username.ToLower().Contains(value) ||
                (x.Phone != null && x.Phone.Contains(value)));
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Customer?> GetByUserIdAsync(int userId)
    {
        return await _context.Customers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}