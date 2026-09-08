using FluentNHibernate.Cfg;
using GlavLib.Db.Providers;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace GlavLib.Db.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Подключает PostgreSQL: диалект NHibernate, конвенции и <see cref="NpgsqlDbSessionFactory"/>
    /// как реализацию <see cref="DbSessionFactory"/>.
    /// </summary>
    [PublicAPI]
    public static IServiceCollection AddNpgsql(
            this IServiceCollection services,
            Action<FluentConfiguration> setup,
            Action<NpgsqlDataSourceProviderOptions>? configureOptions = null
        )
    {
        var options = new NpgsqlDataSourceProviderOptions();
        configureOptions?.Invoke(options);

        var configuration = Fluently.Configure()
                                    .UsePostgreSQL()
                                    .UseNpgsqlDefaults();

        setup(configuration);

        services.AddSingleton(options);
        services.AddSingleton(configuration.BuildSessionFactory());
        services.AddSingleton<NpgsqlDataSourceProvider>();
        services.AddSingleton<DbSessionFactory, NpgsqlDbSessionFactory>();

        return services;
    }

    /// <summary>
    /// Подключает SQLite: диалект NHibernate, конвенции и <see cref="SqliteDbSessionFactory"/>
    /// как реализацию <see cref="DbSessionFactory"/>.
    /// </summary>
    [PublicAPI]
    public static IServiceCollection AddSqlite(
            this IServiceCollection services,
            Action<FluentConfiguration> setup
        )
    {
        var configuration = Fluently.Configure()
                                    .UseSqlite()
                                    .UseSqliteDefaults();

        setup(configuration);

        services.AddSingleton(configuration.BuildSessionFactory());
        services.AddSingleton<DbSessionFactory, SqliteDbSessionFactory>();

        return services;
    }
}
