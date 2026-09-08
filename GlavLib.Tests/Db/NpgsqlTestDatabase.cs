using GlavLib.Db.Extensions;
using GlavLib.Db.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GlavLib.Tests.Db;

/// <summary>
/// Таблица для проверки пользовательских типов в `glavdb`. Создаётся и удаляется тестом:
/// библиотека схемой не занимается, а миграции репозитория к тестовым таблицам отношения
/// не имеют.
/// </summary>
public sealed class NpgsqlTestDatabase : IDisposable
{
    private const string Schema = """
                                  drop table if exists stored_records;

                                  create table stored_records (
                                      id         bigserial primary key,
                                      name       text not null,
                                      created_at timestamp null,
                                      birth_date date null,
                                      period     date null,
                                      duration   bigint null,
                                      payload    text null,
                                      currency   text null
                                  );
                                  """;

    private readonly ServiceProvider _serviceProvider;

    public DbSessionFactory SessionFactory { get; }

    public NpgsqlTestDatabase()
    {
        var configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();

        _serviceProvider = new ServiceCollection()
                           .AddLogging()
                           .AddSingleton<IConfiguration>(configuration)
                           .AddNpgsql(nh => nh.AddFluentMappings("GlavLib.Tests"))
                           .BuildServiceProvider();

        SessionFactory = _serviceProvider.GetRequiredService<DbSessionFactory>();

        Execute(Schema);
    }

    public void Dispose()
    {
        Execute("drop table if exists stored_records;");

        _serviceProvider.Dispose();
    }

    private void Execute(string sql)
    {
        using var dbSession = SessionFactory.OpenStatelessSession(ConnectionStringNames.Master);

        using var command = dbSession.Connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
