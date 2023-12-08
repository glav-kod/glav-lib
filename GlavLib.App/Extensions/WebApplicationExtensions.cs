using App.Metrics;
using GlavLib.Basics.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace GlavLib.App.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder, string applicationName)
    {
        var configuration = builder.Configuration;

        Log.Logger = new LoggerBuilder()
                     .WithApplicationName(applicationName)
                     .Configure(configuration)
                     .Build();

        builder.Services.AddLogging(configure => //
        {
            configure.AddSerilog();
        });

        return builder;
    }

    public static WebApplicationBuilder AddAppMetrics(this WebApplicationBuilder builder)
    {
        var metrics = AppMetrics.CreateDefaultBuilder()
                                .Configuration.Configure(options => //
                                {
                                    options.DefaultContextLabel = "spectr";
                                })
                                .Build();
        builder.Services.AddMetrics(metrics);
        builder.Services.AddMetricsEndpoints();

        //Нужно для AppMetrics
        builder.Services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });

        return builder;
    }

    public static WebApplicationBuilder ConfigureBuilder(this WebApplicationBuilder builder, Action<WebApplicationBuilder> config)
    {
        config(builder);

        return builder;
    }
    
    public static WebApplicationBuilder ConfigureJsonOptions(this WebApplicationBuilder builder, Action<JsonOptions> configureOptions)
    {
        builder.Services.ConfigureHttpJsonOptions(configureOptions);

        return builder;
    }

    public static WebApplicationBuilder Bootstrap(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        return builder;
    }
}