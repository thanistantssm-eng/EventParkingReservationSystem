using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Interfaces.Repositories.Core;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.EntityFrameworkCore;
using System;

namespace EventParkingReservationSystem.API.Repositories.Core;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(
        int id)
    {
        return await _context.Users
            .Include(x => x.Organizer)
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x =>
                x.Id == id);
    }

    public async Task<User?> GetByIdentifierAsync(
        string identifier)
    {
        var value =
            identifier.Trim().ToLower();

        return await _context.Users
            .Include(x => x.Organizer)
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x =>
                x.Username.ToLower() == value ||
                x.Email.ToLower() == value);
    }

    public async Task<bool> UsernameExistsAsync(
        string username)
    {
        var value =
            username.Trim().ToLower();

        return await _context.Users
            .AnyAsync(x =>
                x.Username.ToLower() == value);
    }

    public async Task<bool> EmailExistsAsync(
        string email)
    {
        var value =
            email.Trim().ToLower();

        return await _context.Users
            .AnyAsync(x =>
                x.Email.ToLower() == value);
    }

    public async Task AddAsync(
        User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}