using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace GlavLib.App.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder UseCustomAppSettings(this WebApplicationBuilder appBuilder)
    {
        appBuilder.Configuration.Sources.Clear();
        appBuilder.Configuration.AddJsonFile("appsettings.base.json", optional: false);
        appBuilder.Configuration.AddJsonFile("appsettings.json", optional: true);
        appBuilder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
        appBuilder.Configuration.AddJsonFile("appsettings.production.json", optional: true);
        appBuilder.Configuration.AddEnvironmentVariables();

        return appBuilder;

    }
}