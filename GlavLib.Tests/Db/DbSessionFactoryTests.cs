using GlavLib.Db;
using GlavLib.Db.Extensions;
using GlavLib.Db.Providers;
using GlavLib.Tests.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Xunit.Abstractions;

namespace GlavLib.Tests.Db;

[Collection(nameof(IntegrationTestsCollection))]
public sealed class DbSessionFactoryTests : IDisposable
{
    private readonly DbSessionFactory _dbSessionFactory;
    private readonly ServiceProvider _serviceProvider;

    public DbSessionFactoryTests(ITestOutputHelper testOutputHelper)
    {
        var logger = new LoggerConfiguration()
                     .WriteTo.TestOutput(testOutputHelper)
                     .MinimumLevel.Override("Npgsql.Command", LogEventLevel.Warning)
                     .CreateLogger();

        var configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();

        _serviceProvider = new ServiceCollection()
                           .AddLogging(config => config.AddSerilog(logger))
                           .AddSingleton<IConfiguration>(configuration)
                           .Add_GlavLib_Db()
                           .AddNh(config =>
                           {
                               config.UsePostgreSQL()
                                     .UseDefaults();
                           })
                           .BuildServiceProvider();

        _dbSessionFactory = _serviceProvider.GetRequiredService<DbSessionFactory>();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    [Fact]
    public void DbSession_should_free_connections()
    {
        for (var i = 0; i < 200; i++)
        {
            var dbSession = _dbSessionFactory.OpenSession(ConnectionStringNames.Master);
            dbSession.BeginTransaction();
            dbSession.Commit();
            
            dbSession.BeginTransaction();
            dbSession.Commit();


            dbSession.Dispose();
        }
    }
    
    [Fact]
    public void DbSession_should_open_transaction_many_times()
    {
        using var dbSession = _dbSessionFactory.OpenSession(ConnectionStringNames.Master);
        
        dbSession.BeginTransaction();
        dbSession.Commit();
            
        dbSession.BeginTransaction();
        dbSession.Commit();
    }

    [Fact]
    public void StatelessDbSession_should_free_connections()
    {
        for (var i = 0; i < 200; i++)
        {
            var dbSession = _dbSessionFactory.OpenStatelessSession(ConnectionStringNames.Master);

            dbSession.Dispose();
        }
    }
    
    [Fact]
    public void StatelessDbSession_should_open_transaction_many_times()
    {
        using var dbSession = _dbSessionFactory.OpenStatelessSession(ConnectionStringNames.Master);

        dbSession.BeginTransaction();
        dbSession.Commit();
            
        dbSession.BeginTransaction();
        dbSession.Commit();
    }
}