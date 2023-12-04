using GlavLib.Basics.DomainEvents;

namespace GlavLib.App.DomainEvents;

public interface IDomainEventHandler
{
    Task HandleAsync(DomainEvent domainEvent, CancellationToken cancellationToken);
}

public abstract class DomainEventHandler<TDomainEvent> : IDomainEventHandler
    where TDomainEvent : DomainEvent
{
    public Task HandleAsync(DomainEvent domainEvent, CancellationToken cancellationToken)
    {
        return HandleAsync((TDomainEvent)domainEvent, cancellationToken);
    }

    protected abstract Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken);
}