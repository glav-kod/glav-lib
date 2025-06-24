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
    private readonly object _syncObject = new();

    private readonly Dictionary<string, NpgsqlDataSource> _dataSources = new();

    public NpgsqlDataSource GetDataSource(string connectionStringName)
    {
        lock (_syncObject)
        {
            if (!_dataSources.TryGetValue(connectionStringName, out var dataSource))
            {
                var connectionString = configuration.GetConnectionString(connectionStringName);

                var dsBuilder = new NpgsqlDataSourceBuilder(connectionString)
                {
                    ConnectionStringBuilder =
                    {
                        ApplicationName = npgsqlOptions?.ApplicationName
                    }
                };

                dsBuilder.UseLoggerFactory(loggerFactory);

                dataSource                         = dsBuilder.Build();
                _dataSources[connectionStringName] = dataSource;
            }

            return dataSource;
        }
    }

    public void CloseAllConnections()
    {
        lock (_syncObject)
        {
            foreach (var (_, dataSource) in _dataSources)
                dataSource.Dispose();

            _dataSources.Clear();
        }
    }

    public void Dispose()
    {
        CloseAllConnections();
    }
}