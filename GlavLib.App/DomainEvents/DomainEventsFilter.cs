using GlavLib.Basics.DomainEvents;
using GlavLib.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GlavLib.App.DomainEvents;

public static class DomainEventsFilterExtensions
{
    public static TBuilder UseDomainEvents<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.AddEndpointFilterFactory(DomainEventsFilter.Factory);
    }
}

public sealed class DomainEventsFilter
{
    public static Func<EndpointFilterFactoryContext, EndpointFilterDelegate, EndpointFilterDelegate> Factory => (_, next) => Create(next);

    public static EndpointFilterDelegate Create(EndpointFilterDelegate next)
    {
        return async context =>
        {
            var serviceProvider = context.HttpContext.RequestServices;

            var logger              = serviceProvider.GetRequiredService<ILogger<DomainEventsFilter>>();
            var domainEventsHandler = serviceProvider.GetRequiredService<DomainEventsHandler>();

            using var domainEventsSession = DomainEventsSession.Bind();

            var result = await next(context);
            
            if (!domainEventsSession.IsCommited || domainEventsSession.Events.Count <= 0)
                return result;

            using var dbTransaction = new DbTransaction();

            await domainEventsHandler.HandleAsync(domainEventsSession, context.HttpContext.RequestAborted);

            logger.LogInformation("Обработка ДоменныхСобытий завершилась успешно");

            dbTransaction.Commit();

            return result;
        };
    }
}
