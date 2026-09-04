using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.Interfaces.Services.Core;

public interface IJwtTokenService
{
    string GenerateToken(User user);

    DateTime GetExpirationTime();
}