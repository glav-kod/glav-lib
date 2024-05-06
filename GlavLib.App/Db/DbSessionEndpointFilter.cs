using GlavLib.Db.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GlavLib.App.Db;

public static class DbSessionEndpointFilterExtensions
{
    public static TBuilder AddDbSession<TBuilder>(
            this TBuilder builder,
            string connectionStringName
        )
        where TBuilder : IEndpointConventionBuilder
    {
        var filterFactory = DbSessionEndpointFilter.Factory(connectionStringName);
        return builder.AddEndpointFilterFactory(filterFactory);
    }
}

public static class DbSessionEndpointFilter
{
    public static Func<EndpointFilterFactoryContext, EndpointFilterDelegate, EndpointFilterDelegate> Factory(string connectionStringName)
    {
        return (_, next) => Create(next, connectionStringName);
    }

    public static EndpointFilterDelegate Create(EndpointFilterDelegate next, string connectionStringName)
    {
        return async context =>
        {
            var serviceProvider = context.HttpContext.RequestServices;

            var dbSessionFactory = serviceProvider.GetRequiredService<DbSessionFactory>();

            using (dbSessionFactory.OpenStatefulSession(connectionStringName).Bind())
            {
                return await next(context);
            }
        };
    }
}