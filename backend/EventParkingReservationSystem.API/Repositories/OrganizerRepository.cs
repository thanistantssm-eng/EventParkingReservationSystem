using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Interfaces.Repositories;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Repositories;

public class OrganizerRepository : IOrganizerRepository
{
    private readonly AppDbContext _context;

    public OrganizerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Organizer>> GetAllAsync()
    {
        return await _context
            .Set<Organizer>()
            .Include(x => x.User)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<Organizer?> GetByIdAsync(int id)
    {
        return await _context
            .Set<Organizer>()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Organizer organizer)
    {
        await _context
            .Set<Organizer>()
            .AddAsync(organizer);

        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}