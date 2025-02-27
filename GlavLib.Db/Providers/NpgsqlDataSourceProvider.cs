using System.Collections.Concurrent;
using GlavLib.Abstractions.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace GlavLib.Db.Providers;

public sealed class NpgsqlDataSourceProviderOptions
{
    public string? ApplicationName { get; set; }
}

[SingleInstance]
public sealed class NpgsqlDataSourceProvider(
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        NpgsqlDataSourceProviderOptions? npgsqlOptions = null
    ) : IDisposable
{
    private readonly ConcurrentDictionary<string, NpgsqlDataSource> _dataSources = new();

    public NpgsqlDataSource GetDataSource(string connectionStringName)
    {
        return _dataSources.GetOrAdd(connectionStringName, cs =>
        {
            var connectionString = configuration.GetConnectionString(cs);

            var dsBuilder = new NpgsqlDataSourceBuilder(connectionString)
            {
                ConnectionStringBuilder =
                {
                    ApplicationName = npgsqlOptions?.ApplicationName
                }
            };

            dsBuilder.UseLoggerFactory(loggerFactory);

            return dsBuilder.Build();
        });
    }

    public void Dispose()
    {
        foreach (var (_, dataSource) in _dataSources)
        {
            dataSource.Dispose();
        }
    }
}