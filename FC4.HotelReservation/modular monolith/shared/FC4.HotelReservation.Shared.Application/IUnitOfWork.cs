using FC4.HotelReservation.Shared.Domain;

namespace FC4.HotelReservation.Shared.Application;

public interface IUnitOfWork
{
    void Register(AggregateRoot aggregateRoot);
    Task CommitAsync(CancellationToken cancellationToken);
}