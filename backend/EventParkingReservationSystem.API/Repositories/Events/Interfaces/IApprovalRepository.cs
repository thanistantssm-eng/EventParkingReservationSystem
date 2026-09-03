using EventParkingReservationSystem.API.Models.Events;

namespace EventParkingReservationSystem.API.Repositories.Events.Interfaces;

public interface IApprovalRepository
{
    IQueryable<EventApproval> Query(bool tracking = false);
    Task<EventApproval?> GetByIdAsync(int id, bool tracking = true, CancellationToken cancellationToken = default);
    Task<EventApproval?> GetPendingForEventAsync(int eventId, CancellationToken cancellationToken = default);
    Task AddAsync(EventApproval entity, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
