using System.Reflection;
using Dapper;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Helpers;
using GlavLib.App.Db.Dapper;
using GlavLib.App.Db.NhConventions;
using GlavLib.App.Db.NhUserTypes;
using GlavLib.Basics.DataTypes;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using NHibernate.Dialect;
using Environment = NHibernate.Cfg.Environment;

namespace GlavLib.App.Db;

public static class FluentConfigurationExtensions
{
    static FluentConfigurationExtensions()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        SqlMapper.AddTypeHandler(typeof(Date), new DateHandler());
        SqlMapper.AddTypeHandler(typeof(UtcDateTime), new UtcDateTimeHandler());
    }

    [PublicAPI]
    public static FluentConfiguration AddFluentMappings(this FluentConfiguration fluentConfiguration, string assemblyName)
    {
        var assembly = Assembly.Load(assemblyName);

        fluentConfiguration.Mappings(x => x.FluentMappings.AddFromAssembly(assembly));

        return fluentConfiguration;
    }

    [PublicAPI]
    public static FluentConfiguration Use<TConvention>(this FluentConfiguration fluentConfiguration)
        where TConvention : IConvention
    {
        fluentConfiguration.Mappings(x => x.FluentMappings.Conventions.Add<TConvention>());
        return fluentConfiguration;
    }

    [PublicAPI]
    public static FluentConfiguration UseConfiguration(this FluentConfiguration fluentConfiguration,
                                                       IConfiguration           configuration,
                                                       string                   connectionStringName = "Default")
    {
        var postgreSqlConfiguration = PostgreSQLConfiguration.Standard.Dialect<PostgreSQLDialect>();

        var connectionString = ReadConnectionString(configuration, connectionStringName);
        postgreSqlConfiguration.ConnectionString(connectionString);

        fluentConfiguration.Database(postgreSqlConfiguration);

        return fluentConfiguration;
    }

    [PublicAPI]
    public static FluentConfiguration UseDefaultConventions(this FluentConfiguration fluentConfiguration)
    {
        return fluentConfiguration.Mappings(m =>
                                  {
                                      m.FluentMappings.Conventions.Add(new IdConvention(),
                                                                       new PropertyConvention(),
                                                                       new ReferenceConvention(),
                                                                       new ClassConvention(),
                                                                       new EnumConvention(),
                                                                       new HasManyConvention(),
                                                                       new HasOneConvention(),
                                                                       DefaultAccess.Property(),
                                                                       new UserTypesConventions()
                                      );
                                  })
                                  .ExposeConfiguration(cfg => cfg.SetProperty(Environment.Hbm2ddlKeyWords, "none"));
    }

    private static string ReadConnectionString(IConfiguration configuration, string connectionStringName)
    {
        foreach (var configurationSection in configuration.GetSection("ConnectionStrings").GetChildren())
        {
            if (configurationSection.Key == connectionStringName)
                return configurationSection.Value ?? throw new InvalidOperationException($"ConnectionString '{configurationSection.Key}' is null");
        }

        throw new InvalidOperationException($"Cannot find ConnectionString '{connectionStringName}'");
    }
}