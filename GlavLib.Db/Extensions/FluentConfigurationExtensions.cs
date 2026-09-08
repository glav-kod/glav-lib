using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Helpers;
using GlavLib.Db.NhConventions;
using GlavLib.Db.NhUserTypes;
using JetBrains.Annotations;
using NHibernate.Dialect;
using Environment = NHibernate.Cfg.Environment;

namespace GlavLib.Db.Extensions;

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

    // ReSharper disable once InconsistentNaming
    internal static FluentConfiguration UsePostgreSQL(this FluentConfiguration fluentConfiguration)
    {
        var postgreSqlConfiguration = PostgreSQLConfiguration.Standard.Dialect<PostgreSQLDialect>();
        postgreSqlConfiguration.ConnectionString(string.Empty);
        fluentConfiguration.Database(postgreSqlConfiguration);

        return fluentConfiguration;
    }

    internal static FluentConfiguration UseSqlite(this FluentConfiguration fluentConfiguration)
    {
        //MsSqliteConfiguration сама ставит диалект и драйвер Microsoft.Data.Sqlite.
        var sqliteConfiguration = MsSqliteConfiguration.Standard;
        sqliteConfiguration.ConnectionString(string.Empty);
        fluentConfiguration.Database(sqliteConfiguration);

        return fluentConfiguration;
    }

    internal static FluentConfiguration UseNpgsqlDefaults(this FluentConfiguration fluentConfiguration)
    {
        return fluentConfiguration.UseDefaults(new IdConvention(), new UserTypesConventions());
    }

    internal static FluentConfiguration UseSqliteDefaults(this FluentConfiguration fluentConfiguration)
    {
        return fluentConfiguration.UseDefaults(new SqliteIdConvention(), new SqliteUserTypesConventions());
    }

    /// <summary>
    /// Общий набор конвенций. От СУБД зависят выдача идентификаторов и хранение собственных
    /// типов, поэтому они приходят параметрами: остальные конвенции лишь режут имена
    /// и подставляют <c>EnumType</c>.
    /// </summary>
    private static FluentConfiguration UseDefaults(
            this FluentConfiguration fluentConfiguration,
            IIdConvention idConvention,
            IPropertyConvention userTypesConvention
        )
    {
        return fluentConfiguration.Mappings(m =>
                                  {
                                      m.FluentMappings.Conventions.Add(idConvention,
                                                                       new PropertyConvention(),
                                                                       new ReferenceConvention(),
                                                                       new ClassConvention(),
                                                                       new EnumConvention(),
                                                                       new HasManyConvention(),
                                                                       new HasOneConvention(),
                                                                       DefaultAccess.Property(),
                                                                       userTypesConvention
                                      );
                                  })
                                  .ExposeConfiguration(cfg => cfg.SetProperty(Environment.Hbm2ddlKeyWords, "none"));
    }
}
