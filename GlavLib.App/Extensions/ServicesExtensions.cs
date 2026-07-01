using App.Metrics;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace GlavLib.App.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddDefaults(this IServiceCollection services)
    {
        services.AddRequestTimeouts()
                .AddEndpointsApiExplorer();

        services.AddHealthChecks();

        return services;
    }

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