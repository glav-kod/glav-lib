using FluentNHibernate.Cfg;
using GlavLib.Db.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace GlavLib.Db.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureNpgsql(this IServiceCollection services, Action<NpgsqlDataSourceProviderOptions> configure)
    {
        var options = new NpgsqlDataSourceProviderOptions();
        configure(options);

        services.AddSingleton(options);

        return services;
    }

    public static IServiceCollection AddNh(this IServiceCollection services, Action<FluentConfiguration> setup)
    {
        var configuration = Fluently.Configure();

        setup(configuration);

        var sessionFactory = configuration.BuildSessionFactory();

        services.AddSingleton(sessionFactory);

        return services;
    }
}