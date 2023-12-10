using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Helpers;
using GlavLib.App.Db.NhConventions;
using GlavLib.App.Db.NhUserTypes;
using JetBrains.Annotations;
using NHibernate.Dialect;
using Environment = NHibernate.Cfg.Environment;

namespace GlavLib.App.Db;

public static class FluentConfigurationExtensions
{
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
    public static FluentConfiguration UsePostgreSQL(this FluentConfiguration fluentConfiguration)
    {
        var postgreSqlConfiguration = PostgreSQLConfiguration.Standard.Dialect<PostgreSQLDialect>();
        postgreSqlConfiguration.ConnectionString(string.Empty);
        fluentConfiguration.Database(postgreSqlConfiguration);

        return fluentConfiguration;
    }

    [PublicAPI]
    public static FluentConfiguration UseDefaults(this FluentConfiguration fluentConfiguration)
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
}