using GlavLib.Abstractions.DataTypes;
using GlavLib.Abstractions.Validation;
using GlavLib.Basics.DataTypes;
using GlavLib.Basics.Logging.Enrichers;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;

namespace GlavLib.Basics.Logging;

public sealed class LoggerBuilder
{
    private readonly LoggerConfiguration _loggerConfiguration = new();

    public LoggerBuilder()
    {
        _loggerConfiguration.Destructure.ByTransforming<TimeZoneInfo>(d => d.Id)
                            .Destructure.ByTransforming<Date>(d => d.Value)
                            .Destructure.ByTransforming<UtcDateTime>(d => d.Value)
                            .Destructure.ByTransformingWhere<EnumObject>(x => typeof(EnumObject).IsAssignableFrom(x), o => o.Key)
                            .Destructure.ByTransforming<Error>(d => new
                            {
                                d.Key,
                                d.Message,
                                d.Args
                            })
                            .Enrich.WithExceptionDetails()
                            .Enrich.WithMachineName()
                            .Enrich.FromLogContext()
                            .Enrich.With<ClassNameEnricher>();
    }

    public LoggerBuilder WithApplicationName(string applicationName)
    {
        _loggerConfiguration.Enrich.WithProperty("Application", applicationName);
        return this;
    }

    public LoggerBuilder Configure(Action<LoggerConfiguration> configure)
    {
        configure(_loggerConfiguration);
        return this;
    }

    public LoggerBuilder Configure(IConfiguration configuration)
    {
        _loggerConfiguration.ReadFrom.Configuration(configuration);
        return this;
    }

    public Logger Build()
    {
        return _loggerConfiguration.CreateLogger();
    }
}