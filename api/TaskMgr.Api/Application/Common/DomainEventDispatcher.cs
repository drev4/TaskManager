using MediatR;
using TaskMgr.Api.Domain.Entities;

namespace TaskMgr.Api.Application.Common;

/// <summary>
/// Publishes and clears the domain events recorded on an entity after it has been persisted
/// </summary>
public static class DomainEventDispatcher
{
    public static async Task PublishAndClearAsync(IPublisher publisher, BaseEntity entity, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in entity.DomainEvents)
        {
            await publisher.Publish(domainEvent, cancellationToken);
        }

        entity.ClearDomainEvents();
    }
}
