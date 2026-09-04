using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Interfaces.Repositories.Core;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.EntityFrameworkCore;
using System;

namespace EventParkingReservationSystem.API.Repositories.Core;

public class OrganizerRepository
    : IOrganizerRepository
{
    private readonly AppDbContext _context;

    public OrganizerRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Organizer>>
        GetAllAsync()
    {
        return await _context.Organizers
            .Include(x => x.User)
            .AsNoTracking()
            .OrderByDescending(x =>
                x.CreatedAt)
            .ToListAsync();
    }

    public async Task<Organizer?>
        GetByIdAsync(int id)
    {
        return await _context.Organizers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.Id == id);
    }

    public async Task<Organizer?>
        GetByUserIdAsync(int userId)
    {
        return await _context.Organizers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.UserId == userId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}