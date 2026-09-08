using GlavLib.Db.Dapper;
using GlavLib.Db.Extensions;
using GlavLib.Db.Providers;
using GlavLib.Tests.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GlavLib.Tests.Sqlite;

/// <summary>
/// Файловая база SQLite на время одного класса тестов. Схему создаёт сам тест: библиотека
/// созданием схемы не занимается ни для PostgreSQL, ни для SQLite.
/// </summary>
public sealed class SqliteTestDatabase : IDisposable
{
    public const string ConnectionStringName = "sqlite";

    private const string Schema = """
                                  CREATE TABLE stored_records (
                                      id         INTEGER PRIMARY KEY AUTOINCREMENT,
                                      name       TEXT NOT NULL,
                                      created_at TEXT NULL,
                                      birth_date TEXT NULL,
                                      period     TEXT NULL,
                                      duration   TEXT NULL,
                                      payload    TEXT NULL,
                                      currency   TEXT NULL
                                  );

                                  CREATE TABLE child_records (
                                      id               INTEGER PRIMARY KEY AUTOINCREMENT,
                                      stored_record_id INTEGER NOT NULL REFERENCES stored_records (id)
                                  );
                                  """;

    private readonly string _filePath;
    private readonly ServiceProvider _serviceProvider;

    public DbSessionFactory SessionFactory { get; }

    public SqliteTestDatabase()
    {
        DapperConventions.SetupSqlite();

        _filePath = Path.Combine(Path.GetTempPath(), $"glavlib-{Guid.NewGuid():N}.sqlite");

        var configuration = new ConfigurationBuilder()
                            .AddInMemoryCollection(new Dictionary<string, string?>
                            {
                                [$"ConnectionStrings:{ConnectionStringName}"] = $"Data Source={_filePath}"
                            })
                            .Build();

        _serviceProvider = new ServiceCollection()
                           .AddSingleton<IConfiguration>(configuration)
                           .AddSqlite(nh => nh.AddFluentMappings("GlavLib.Tests"))
                           .BuildServiceProvider();

        SessionFactory = _serviceProvider.GetRequiredService<DbSessionFactory>();

        CreateSchema();
    }

    private void CreateSchema()
    {
        using var dbSession = SessionFactory.OpenStatelessSession(ConnectionStringName);

        using var command = dbSession.Connection.CreateCommand();
        command.CommandText = Schema;
        command.ExecuteNonQuery();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();

        //Пул держит файл открытым, поэтому его нужно опустошить до удаления файла.
        SessionFactory.CloseAllConnections();

        File.Delete(_filePath);
    }
}
