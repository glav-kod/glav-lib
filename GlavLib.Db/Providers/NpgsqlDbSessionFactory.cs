using NHibernate;

namespace GlavLib.Db.Providers;

public sealed class NpgsqlDbSessionFactory(
        ISessionFactory sessionFactory,
        NpgsqlDataSourceProvider npgsqlDataSourceProvider
    ) : DbSessionFactory
{
    public override StatefulDbSession OpenStatefulSession(string connectionStringName)
    {
        var npgsqlDataSource = npgsqlDataSourceProvider.GetDataSource(connectionStringName);

        var dbConnection = npgsqlDataSource.OpenConnection();

        var session = sessionFactory.WithOptions()
                                    .Connection(dbConnection)
                                    .OpenSession();

        return new StatefulDbSession(session);
    }

    public override StatelessDbSession OpenStatelessSession(string connectionStringName)
    {
        var npgsqlDataSource = npgsqlDataSourceProvider.GetDataSource(connectionStringName);

        var dbConnection = npgsqlDataSource.OpenConnection();

        var session = sessionFactory.OpenStatelessSession(dbConnection);

        return new StatelessDbSession(session);
    }

    public override void CloseAllConnections()
    {
        npgsqlDataSourceProvider.CloseAllConnections();
    }
}
