using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.Interfaces.Repositories.Core;

public interface ILoginOtpRepository
{
    Task<LoginOtp?> GetByChallengeIdAsync(
        Guid challengeId);

    Task AddAsync(LoginOtp loginOtp);

    Task InvalidateActiveOtpsAsync(
        int userId);

    Task SaveChangesAsync();
}