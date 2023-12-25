using GlavLib.Db;
using GlavLib.Db.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;

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

            var dataSourceProvider = serviceProvider.GetRequiredService<NpgsqlDataSourceProvider>();
            var sessionFactory     = serviceProvider.GetRequiredService<ISessionFactory>();

            var npgsqlDataSource = dataSourceProvider.GetDataSource(connectionStringName);

            await using var dbConnection = await npgsqlDataSource.OpenConnectionAsync();

            await using (DbSession.Bind(sessionFactory, dbConnection))
            {
                return await next(context);
            }
        };
    }
}