using Dapper;
using GlavLib.Basics.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Serilog;

var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile("appsettings.Development.json", optional: true)
                    .Build();

Log.Logger = new LoggerBuilder()
             .Configure(configuration)
             .Build();

var services = new ServiceCollection();
services.AddLogging(configure => //
{
    configure.AddSerilog();
});

var serviceProvider = services.BuildServiceProvider();

var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

var dsBuilder = new NpgsqlDataSourceBuilder("User ID=sys;Password=123;Host=127.100.0.1;Port=5432;Database=glavdb");
dsBuilder.UseLoggerFactory(loggerFactory);

var dataSource = dsBuilder.Build();

var connection = dataSource.OpenConnection();

const string sql = "select * from test_table";

var result = connection.Query(sql).ToList();

Console.WriteLine(result.Count);
    
foreach (var row in result.Take(10))
{
    Console.WriteLine(row);
}

Log.CloseAndFlush();