using EventParkingReservationSystem.API.Models.Events;

namespace EventParkingReservationSystem.API.Repositories.Events.Interfaces;

public interface ITicketRepository
{
    IQueryable<TicketType> Query(bool tracking = false);
    Task<TicketType?> GetByIdAsync(int id, bool tracking = true, CancellationToken cancellationToken = default);
    Task AddAsync(TicketType entity, CancellationToken cancellationToken = default);
    void Remove(TicketType entity);
    Task<bool> NameExistsAsync(int eventId, string name, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
