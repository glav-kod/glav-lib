using Microsoft.Extensions.Configuration;

namespace GlavLib.App.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static TConfigurationBuilder UseCustomAppSettings<TConfigurationBuilder>(this TConfigurationBuilder configurationBuilder)
        where TConfigurationBuilder : IConfigurationBuilder
    {
        configurationBuilder.Sources.Clear();
        configurationBuilder.AddJsonFile("appsettings.base.json", optional: false);
        configurationBuilder.AddJsonFile("appsettings.json", optional: true);
        configurationBuilder.AddJsonFile("appsettings.local.json", optional: true);
        configurationBuilder.AddJsonFile("appsettings.production.json", optional: true);
        configurationBuilder.AddEnvironmentVariables();

        return configurationBuilder;
    }
}