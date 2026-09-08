using Dapper;
using FluentAssertions;
using GlavLib.Basics.DataTypes;
using GlavLib.Db;
using Microsoft.Data.Sqlite;

namespace GlavLib.Tests.Sqlite;

public sealed class SqliteDbSessionTests : IClassFixture<SqliteTestDatabase>
{
    private readonly SqliteTestDatabase _database;

    public SqliteDbSessionTests(SqliteTestDatabase database)
    {
        _database = database;
    }

    private StatefulDbSession OpenSession()
    {
        return _database.SessionFactory.OpenStatefulSession(SqliteTestDatabase.ConnectionStringName);
    }

    [Fact]
    public void It_should_assign_identifier_on_insert()
    {
        using var dbSession = OpenSession();
        using var transaction = new DbTransaction(dbSession);

        var record = new StoredRecord { Name = "identity" };

        dbSession.NhSession.Save(record);
        dbSession.NhSession.Flush();

        record.Id.Should().BeGreaterThan(0);

        transaction.Commit();
    }

    [Fact]
    public void It_should_roundtrip_all_supported_types()
    {
        var createdAt = new UtcDateTime(2026, 9, 8, 14, 35, 12);
        var birthDate = new Date(1990, 5, 17);
        var period = new YearMonth(2026, 1);
        var duration = TimeSpan.FromMinutes(97);

        long id;

        using (var dbSession = OpenSession())
        using (var transaction = new DbTransaction(dbSession))
        {
            var record = new StoredRecord
            {
                Name      = "roundtrip",
                CreatedAt = createdAt,
                BirthDate = birthDate,
                Period    = period,
                Duration  = duration,
                Payload   = new TestPayload { Title = "полезная нагрузка", Count = 42 },
                Currency  = TestCurrency.Som
            };

            dbSession.NhSession.Save(record);
            transaction.Commit();

            id = record.Id;
        }

        using (var dbSession = OpenSession())
        {
            var record = dbSession.NhSession.Get<StoredRecord>(id);

            record.Name.Should().Be("roundtrip");
            record.CreatedAt.Should().Be(createdAt);
            record.BirthDate.Should().Be(birthDate);
            record.Period.Should().Be(period);
            record.Duration.Should().Be(duration);
            record.Payload!.Title.Should().Be("полезная нагрузка");
            record.Payload.Count.Should().Be(42);
            record.Currency.Should().Be(TestCurrency.Som);
        }
    }

    [Fact]
    public void It_should_rollback_transaction_when_it_is_not_committed()
    {
        using (var dbSession = OpenSession())
        using (new DbTransaction(dbSession))
        {
            dbSession.NhSession.Save(new StoredRecord { Name = "rolled back" });
            dbSession.NhSession.Flush();
        }

        using (var dbSession = OpenSession())
        {
            var count = dbSession.Connection.ExecuteScalar<long>(
                "select count(*) from stored_records where name = 'rolled back'");

            count.Should().Be(0);
        }
    }

    [Fact]
    public void It_should_read_supported_types_through_dapper()
    {
        var createdAt = new UtcDateTime(2026, 3, 1, 8, 0, 0);
        var birthDate = new Date(2001, 12, 31);
        var period = new YearMonth(2025, 7);

        long id;

        using (var dbSession = OpenSession())
        using (var transaction = new DbTransaction(dbSession))
        {
            var record = new StoredRecord
            {
                Name      = "dapper",
                CreatedAt = createdAt,
                BirthDate = birthDate,
                Period    = period
            };

            dbSession.NhSession.Save(record);
            transaction.Commit();

            id = record.Id;
        }

        using (var dbSession = OpenSession())
        {
            var row = dbSession.Connection.QuerySingle<StoredRecordRow>(
                "select name, created_at, birth_date, period from stored_records where id = @id",
                new { id });

            row.Name.Should().Be("dapper");
            row.CreatedAt.Should().Be(createdAt);
            row.BirthDate.Should().Be(birthDate);
            row.Period.Should().Be(period);
        }
    }

    [Fact]
    public void It_should_enforce_foreign_keys()
    {
        using var dbSession = OpenSession();

        var insertOrphan = () => dbSession.Connection.Execute(
            "insert into child_records (stored_record_id) values (999999)");

        insertOrphan.Should()
                    .Throw<SqliteException>()
                    .Which.SqliteErrorCode.Should().Be(19); //SQLITE_CONSTRAINT
    }

    [Fact]
    public void It_should_open_stateless_session()
    {
        using var dbSession = _database.SessionFactory
                                       .OpenStatelessSession(SqliteTestDatabase.ConnectionStringName);

        dbSession.BeginTransaction();
        dbSession.Commit();

        dbSession.Connection.State.Should().Be(System.Data.ConnectionState.Open);
    }

    [Fact]
    public void It_should_open_transaction_many_times()
    {
        using var dbSession = OpenSession();

        dbSession.BeginTransaction();
        dbSession.Commit();

        dbSession.BeginTransaction();
        dbSession.Commit();
    }

    private sealed class StoredRecordRow
    {
        public string Name { get; set; } = null!;

        public UtcDateTime CreatedAt { get; set; } = null!;

        public Date BirthDate { get; set; } = null!;

        public YearMonth Period { get; set; } = null!;
    }
}
