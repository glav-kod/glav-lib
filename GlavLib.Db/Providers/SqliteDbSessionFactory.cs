using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using NHibernate;

namespace GlavLib.Db.Providers;

public sealed class SqliteDbSessionFactory(
        ISessionFactory sessionFactory,
        IConfiguration configuration
    ) : DbSessionFactory
{
    public override StatefulDbSession OpenStatefulSession(string connectionStringName)
    {
        var dbConnection = OpenConnection(connectionStringName);

        var session = sessionFactory.WithOptions()
                                    .Connection(dbConnection)
                                    .OpenSession();

        return new StatefulDbSession(session);
    }

    public override StatelessDbSession OpenStatelessSession(string connectionStringName)
    {
        var dbConnection = OpenConnection(connectionStringName);

        var session = sessionFactory.OpenStatelessSession(dbConnection);

        return new StatelessDbSession(session);
    }

    public override void CloseAllConnections()
    {
        SqliteConnection.ClearAllPools();
    }

    private SqliteConnection OpenConnection(string connectionStringName)
    {
        var connectionString = ConnectionStringResolver.Resolve(configuration, connectionStringName);

        var connection = new SqliteConnection(connectionString);
        connection.Open();

        //Проверка внешних ключей в SQLite выключена по умолчанию и задаётся на соединение,
        //а не на файл базы, поэтому включать её нужно каждый раз заново.
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON";
        command.ExecuteNonQuery();

        return connection;
    }
}
