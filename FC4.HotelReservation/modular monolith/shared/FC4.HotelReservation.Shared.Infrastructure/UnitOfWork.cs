using FC4.HotelReservation.Shared.Application;
using FC4.HotelReservation.Shared.Application.Exceptions;
using FC4.HotelReservation.Shared.Domain;
using FC4.HotelReservation.Shared.Infrastructure.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FC4.HotelReservation.Shared.Infrastructure;

public class UnitOfWork(
    HotelDbContext dbContext,
    IPublisher publisher
    ) : IUnitOfWork
{
    private readonly IDbContextTransaction _transaction = dbContext.Database.BeginTransaction();

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        var aggregateRoots = dbContext
            .ChangeTracker
            .Entries<AggregateRoot>()
            .Where(entry => entry.Entity.Events.Count > 0)
            .Select(entry => entry.Entity)
            .ToList();

        foreach (var aggregateRoot in aggregateRoots)
        {
            var events = aggregateRoot.Events.ToList();
            foreach (var @event in events)
            {
                if (aggregateRoot is EventSourced)
                {
                    dbContext.EventStore.Add(EventEntry.FromDomainEvent(@event));
                }
                await publisher.Publish((object)@event, cancellationToken);
                aggregateRoot.RemoveEvent(@event);
            }
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            
            throw new ConflictException("A concurrency conflict occurred while saving changes to the database.", ex);
        }
        finally
        {
            await _transaction.DisposeAsync();
        }
    }
}