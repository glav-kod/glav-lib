using GlavLib.Abstractions.DI;
using NHibernate;

namespace GlavLib.Db.Providers;

[SingleInstance]
public sealed class DbSessionFactory(
        ISessionFactory sessionFactory,
        NpgsqlDataSourceProvider npgsqlDataSourceProvider
    )
{
    public DbSession OpenSession(string connectionStringName)
    {
        var npgsqlDataSource = npgsqlDataSourceProvider.GetDataSource(connectionStringName);

        var dbConnection = npgsqlDataSource.OpenConnection();

        var session = sessionFactory.WithOptions()
                                    .Connection(dbConnection)
                                    .OpenSession();

        return new DbSession(session);
    }

    public StatelessDbSession OpenStatelessSession(string connectionStringName)
    {
        var npgsqlDataSource = npgsqlDataSourceProvider.GetDataSource(connectionStringName);

        var dbConnection = npgsqlDataSource.OpenConnection();

        var session = sessionFactory.OpenStatelessSession(dbConnection);

        return new StatelessDbSession(session);
    }
}