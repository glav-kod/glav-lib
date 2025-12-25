using Destructurama.Attributed;
using GlavLib.Basics.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

var logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("Foo: {@Foo}", new Foo
{
    Value    = "123",
    Password = "pass!@#",
    Text = "text"
});

Log.CloseAndFlush();


public sealed class Foo
{
    public required string Value { get; init; }
 
    [LogMasked]
    public required string Password { get; init; }
    
    [NotLogged]
    public required string Text { get; init; }
}