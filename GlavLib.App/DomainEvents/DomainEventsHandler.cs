using GlavLib.Abstractions.DI;
using GlavLib.Basics.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace GlavLib.App.DomainEvents;

[SingleInstance]
public sealed class DomainEventsHandler
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventsHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task HandleAsync(DomainEventsSession session, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in session.Events)
        {
            var domainEventHandler = _serviceProvider.GetRequiredKeyedService<IDomainEventHandler>(domainEvent.GetType());

            await domainEventHandler.HandleAsync(domainEvent, cancellationToken);
        }
    }
}