using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Interfaces.Repositories;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.EntityFrameworkCore;

namespace EventParkingReservationSystem.API.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly AppDbContext _context;

    public PropertyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Property>> GetAllAsync()
    {
        return await _context
            .Set<Property>()
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Property?> GetByIdAsync(int id)
    {
        return await _context
            .Set<Property>()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Property property)
    {
        await _context
            .Set<Property>()
            .AddAsync(property);

        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}