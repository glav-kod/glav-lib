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
using NHibernate;
using NHibernate.Dialect;
using Configuration = NHibernate.Cfg.Configuration;
using Environment = NHibernate.Cfg.Environment;

namespace GlavLib.App.Db;

public class SessionFactoryBuilder
{
    private string _connectionStringName = ConnectionStringsManager.DefaultConnectionStringName;
    private string? _connectionString;

    private string? _exportMappingsTo;
    private Action<Configuration>? _nhConfig;

    private readonly IList<Assembly> _fluentMappings = new List<Assembly>();
    private readonly IList<Assembly> _hbmMappings = new List<Assembly>();
    private readonly IList<IConvention> _conventions = new List<IConvention>();

    static SessionFactoryBuilder()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        
        SqlMapper.AddTypeHandler(typeof(Date), new DateHandler());
        SqlMapper.AddTypeHandler(typeof(UtcDateTime), new UtcDateTimeHandler());
    }

    [PublicAPI]
    public SessionFactoryBuilder AddFluentMappingsFrom(string assemblyName)
    {
        _fluentMappings.Add(Assembly.Load(assemblyName));

        return this;
    }

    [PublicAPI]
    public SessionFactoryBuilder AddFluentMappingsFrom(Assembly assembly)
    {
        _fluentMappings.Add(assembly);

        return this;
    }

    [PublicAPI]
    public SessionFactoryBuilder AddHbmMappingsFrom(string assemblyName)
    {
        _hbmMappings.Add(Assembly.Load(assemblyName));

        return this;
    }

    [PublicAPI]
    public SessionFactoryBuilder ExportMappingsTo(string path)
    {
        _exportMappingsTo = path;

        return this;
    }

    [PublicAPI]
    public SessionFactoryBuilder SetConnectionString(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }

    [PublicAPI]
    public SessionFactoryBuilder SetConnectionStringName(string connectionStringName)
    {
        _connectionStringName = connectionStringName;
        return this;
    }

    [PublicAPI]
    public SessionFactoryBuilder Use(IConvention convention)
    {
        _conventions.Add(convention);
        return this;
    }

    [PublicAPI]
    public SessionFactoryBuilder ExposeConfiguration(Action<Configuration> configuration)
    {
        _nhConfig = configuration;
        return this;
    }

    [PublicAPI]
    public ISessionFactory Build()
    {
        var postgreSqlConfiguration = PostgreSQLConfiguration.Standard.Dialect<PostgreSQLDialect>();

        var connectionString = _connectionString ?? ConnectionStringsManager.Get(_connectionStringName);

        postgreSqlConfiguration.ConnectionString(connectionString);

        var configuration = Fluently.Configure()
                                    .Database(postgreSqlConfiguration)
                                    .Mappings(m =>
                                    {
                                        foreach (var fluentMapping in _fluentMappings)
                                            m.FluentMappings.AddFromAssembly(fluentMapping);

                                        foreach (var hbmMapping in _hbmMappings)
                                            m.HbmMappings.AddFromAssembly(hbmMapping);

                                        if (!string.IsNullOrEmpty(_exportMappingsTo))
                                        {
                                            m.FluentMappings.ExportTo(_exportMappingsTo);
                                            m.AutoMappings.ExportTo(_exportMappingsTo);
                                        }

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

                                        foreach (var convention in _conventions)
                                            m.FluentMappings.Conventions.Add(convention);
                                    });

        configuration.ExposeConfiguration(cfg => cfg.SetProperty(Environment.Hbm2ddlKeyWords, "none"));

        if (_nhConfig != null)
            configuration.ExposeConfiguration(_nhConfig);

        var sessionFactory = configuration.BuildSessionFactory();

        return sessionFactory;
    }
}