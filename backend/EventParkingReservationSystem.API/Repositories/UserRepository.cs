using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Interfaces.Repositories;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context
            .Set<User>()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<User?> GetByIdentifierAsync(string identifier)
    {
        var value = identifier.Trim().ToLower();

        return await _context
            .Set<User>()
            .FirstOrDefaultAsync(x =>
                x.Username.ToLower() == value ||
                x.Email.ToLower() == value);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var value = email.Trim().ToLower();

        return await _context
            .Set<User>()
            .AnyAsync(x =>
                x.Email.ToLower() == value);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        var value = username.Trim().ToLower();

        return await _context
            .Set<User>()
            .AnyAsync(x =>
                x.Username.ToLower() == value);
    }

    public async Task AddAsync(User user)
    {
        await _context
            .Set<User>()
            .AddAsync(user);

        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}