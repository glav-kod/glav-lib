﻿using GlavLib.Basics.DataTypes;
using GlavLib.Basics.Errors;
using Microsoft.Extensions.Configuration;
using Serilog;
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
                            .Destructure.ByTransforming<ErrorMessage>(d => new
                            {
                                d.Key,
                                d.Message,
                                d.Args
                            })
                            .Enrich.WithExceptionDetails()
                            .Enrich.WithMachineName()
                            .Enrich.FromLogContext();
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

    public ILogger Build()
    {
        return _loggerConfiguration.CreateLogger();
    }
}