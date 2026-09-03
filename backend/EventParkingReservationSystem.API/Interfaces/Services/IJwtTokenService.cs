using EventParkingReservationSystem.API.Helpers;
using EventParkingReservationSystem.API.Models.Core;

namespace EventParkingReservationSystem.API.Interfaces.Services;

public interface IJwtTokenService
{
    TokenResult CreateToken(User user);
}