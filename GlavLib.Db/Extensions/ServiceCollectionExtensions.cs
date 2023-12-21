using FluentNHibernate.Cfg;
using Microsoft.Extensions.DependencyInjection;

namespace GlavLib.Db.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNh(this IServiceCollection services, Action<FluentConfiguration> setup)
    {
        var configuration = Fluently.Configure();

        setup(configuration);

        var sessionFactory = configuration.BuildSessionFactory();

        services.AddSingleton(sessionFactory);

        return services;
    }
}