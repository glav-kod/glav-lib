using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace GlavLib.Db.Providers;

public sealed class NpgsqlDataSourceProvider : IDisposable
{
    private readonly ConcurrentDictionary<string, NpgsqlDataSource> _dataSources = new();

    private readonly ILoggerFactory _loggerFactory;
    private readonly IConfiguration _configuration;

    public NpgsqlDataSourceProvider(ILoggerFactory loggerFactory,
                                    IConfiguration configuration)
    {
        _loggerFactory = loggerFactory;
        _configuration = configuration;
    }

    public NpgsqlDataSource GetDataSource(string connectionStringName)
    {
        return _dataSources.GetOrAdd(connectionStringName, cs =>
        {
            var connectionString = _configuration.GetConnectionString(cs);

            var dsBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dsBuilder.UseLoggerFactory(_loggerFactory);

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