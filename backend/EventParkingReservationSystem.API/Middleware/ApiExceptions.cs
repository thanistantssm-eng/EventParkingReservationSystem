namespace EventParkingReservationSystem.API.Middleware;

public class ApiException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public sealed class NotFoundException(string message) : ApiException(404, message);
public sealed class ConflictException(string message) : ApiException(409, message);
public sealed class ValidationException(string message) : ApiException(400, message);
