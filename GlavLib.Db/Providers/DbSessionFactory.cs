using System.Data;
using GlavLib.Abstractions.DI;
using NHibernate;

namespace GlavLib.Db.Providers;

[SingleInstance]
public sealed class DbSessionFactory(
        ISessionFactory sessionFactory,
        NpgsqlDataSourceProvider npgsqlDataSourceProvider
    )
{
    public DbSession OpenDbSession(string connectionStringName)
    {
        var npgsqlDataSource = npgsqlDataSourceProvider.GetDataSource(connectionStringName);

        var dbConnection = npgsqlDataSource.OpenConnection();

        var session = sessionFactory.WithOptions()
                                    .Connection(dbConnection)
                                    .OpenSession();

        return new DbSession(session);
    }

    public ISession OpenNhSession(string connectionStringName)
    {
        var npgsqlDataSource = npgsqlDataSourceProvider.GetDataSource(connectionStringName);

        var dbConnection = npgsqlDataSource.OpenConnection();

        return sessionFactory.WithOptions()
                             .Connection(dbConnection)
                             .OpenSession();
    }

    public StatelessSessionWrapper OpenStatelessSession(string connectionStringName)
    {
        var npgsqlDataSource = npgsqlDataSourceProvider.GetDataSource(connectionStringName);

        var dbConnection = npgsqlDataSource.OpenConnection();
        var session      = sessionFactory.OpenStatelessSession(dbConnection);

        return new StatelessSessionWrapper(session: session,
                                           connection: dbConnection);
    }

    public sealed class StatelessSessionWrapper(
            IStatelessSession session,
            IDbConnection connection
        ) : IDisposable
    {
        public IStatelessSession Session { get; } = session;

        public IDbConnection Connection { get; } = connection;

        public void Dispose()
        {
            Session.Dispose();
            Connection.Dispose();
        }
    }
}