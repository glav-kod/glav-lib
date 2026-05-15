using GlavLib.Abstractions.DI;
using GlavLib.Basics.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace GlavLib.App.DomainEvents;

[SingleInstance]
public sealed class DomainEventsHandler(IServiceProvider serviceProvider)
{
    public async Task HandleAsync(DomainEventsSession session, CancellationToken cancellationToken)
    {
        if (!session.IsCommited)
            throw new InvalidOperationException("DomainEventSession must be committed before it can be handled");
        
        foreach (var domainEvent in session.Events)
        {
            var domainEventHandler = serviceProvider.GetRequiredKeyedService<IDomainEventHandler>(domainEvent.GetType());

            await domainEventHandler.HandleAsync(domainEvent, cancellationToken);
        }
    }
}