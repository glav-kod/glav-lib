using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NHibernate;

namespace GlavLib.App.Db;

public static class DbSessionEndpointFilterExtensions
{
    public static RouteHandlerBuilder AddDbSession(this RouteHandlerBuilder routeHandlerBuilder)
    {
        return routeHandlerBuilder.AddEndpointFilter<DbSessionEndpointFilter>();
    }

    public static RouteGroupBuilder AddDbSession(this RouteGroupBuilder routeGroupBuilder)
    {
        return routeGroupBuilder.AddEndpointFilter<DbSessionEndpointFilter>();
    }
}

public sealed class DbSessionEndpointFilter : IEndpointFilter
{
    private readonly ISessionFactory _sessionFactory;

    public DbSessionEndpointFilter(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        using (DbSession.Bind(_sessionFactory))
        {
            return await next(context);
        }
    }
}