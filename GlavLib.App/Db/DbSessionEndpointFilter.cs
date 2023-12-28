using GlavLib.Db.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace GlavLib.App.Db;

public static class DbSessionEndpointFilterExtensions
{
    public static RouteHandlerBuilder AddDbSession(this RouteHandlerBuilder routeHandlerBuilder,
                                                   string                   connectionStringName)
    {
        return routeHandlerBuilder.AddEndpointFilterFactory((_, next) => DbSessionEndpointFilter.Create(next, connectionStringName));
    }

    public static RouteGroupBuilder AddDbSession(this RouteGroupBuilder routeGroupBuilder,
                                                 string                 connectionStringName)
    {
        return routeGroupBuilder.AddEndpointFilterFactory((_, next) => DbSessionEndpointFilter.Create(next, connectionStringName));
    }
}

internal sealed class DbSessionEndpointFilter
{
    public static EndpointFilterDelegate Create(EndpointFilterDelegate next, string connectionStringName)
    {
        return async context =>
        {
            var serviceProvider = context.HttpContext.RequestServices;

            var dbSessionFactory = serviceProvider.GetRequiredService<DbSessionFactory>();

            using (dbSessionFactory.OpenSession(connectionStringName).Bind())
            {
                return await next(context);
            }
        };
    }
}