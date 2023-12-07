using Autofac;
using GlavLib.Abstractions.DI;
using GlavLib.Basics.DomainEvents;

namespace GlavLib.App.DomainEvents;

[SingleInstance]
public sealed class DomainEventsHandler
{
    private readonly ILifetimeScope _lifetimeScope;

    public DomainEventsHandler(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    public async Task HandleAsync(DomainEventsSession session, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in session.Events)
        {
            var domainEventHandler = _lifetimeScope.ResolveKeyed<IDomainEventHandler>(domainEvent.GetType());

            await domainEventHandler.HandleAsync(domainEvent, cancellationToken);
        }
    }
}