using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using Npgsql;

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

            var configuration  = serviceProvider.GetRequiredService<IConfiguration>();
            var sessionFactory = serviceProvider.GetRequiredService<ISessionFactory>();

            var connectionString = configuration.GetConnectionString(connectionStringName);

            await using var dbConnection = new NpgsqlConnection(connectionString);
            await dbConnection.OpenAsync();

            await using (DbSession.Bind(sessionFactory, dbConnection))
            {
                return await next(context);
            }
        };
    }
}