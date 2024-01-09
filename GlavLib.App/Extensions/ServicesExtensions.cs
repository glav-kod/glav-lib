using App.Metrics;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace GlavLib.App.Extensions;

public static class ServicesExtensions
{
    /// <summary>
    /// Adds
    /// <list>
    /// <item><c>RequestTimeouts</c></item>
    /// <item><c>AddEndpointsApiExplorer</c></item>
    /// <item><c>AddSwaggerGen</c></item>
    /// <item><c>HealthChecks</c></item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddDefaults(this IServiceCollection services)
    {
        services.AddRequestTimeouts()
                .AddEndpointsApiExplorer()
                .AddSwaggerGen();

        services.AddHealthChecks();

        return services;
    }

    /// <summary>
    /// Adds
    /// <list>
    /// <item><c>Metrics</c></item>
    /// <item><c>MetricsEndpoints</c></item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddAppMetrics(this IServiceCollection services, string defaultContextLabel)
    {
        var metrics = AppMetrics.CreateDefaultBuilder()
                                .Configuration.Configure(options => //
                                {
                                    options.DefaultContextLabel = defaultContextLabel;
                                })
                                .Build();
        services.AddMetrics(metrics);
        services.AddMetricsEndpoints();

        //Нужно для AppMetrics
        services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });

        return services;
    }
}