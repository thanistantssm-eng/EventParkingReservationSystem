using EventParkingReservationSystem.API.Data;
using EventParkingReservationSystem.API.Interfaces.Repositories.Core;
using EventParkingReservationSystem.API.Models.Core;
using Microsoft.EntityFrameworkCore;
using System;

namespace EventParkingReservationSystem.API.Repositories.Core;

public class LoginOtpRepository
    : ILoginOtpRepository
{
    private readonly AppDbContext _context;

    public LoginOtpRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<LoginOtp?>
        GetByChallengeIdAsync(
            Guid challengeId)
    {
        return await _context.LoginOtps
            .Include(x => x.User)
                .ThenInclude(x => x.Organizer)
            .Include(x => x.User)
                .ThenInclude(x => x.Customer)
            .FirstOrDefaultAsync(x =>
                x.ChallengeId == challengeId);
    }

    public async Task AddAsync(
        LoginOtp loginOtp)
    {
        await _context.LoginOtps
            .AddAsync(loginOtp);
    }

    public async Task InvalidateActiveOtpsAsync(
        int userId)
    {
        var activeOtps =
            await _context.LoginOtps
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsUsed)
                .ToListAsync();

        foreach (var otp in activeOtps)
        {
            otp.IsUsed = true;
            otp.UsedAt = DateTime.UtcNow;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}