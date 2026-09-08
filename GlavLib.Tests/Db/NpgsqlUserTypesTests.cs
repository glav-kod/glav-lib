using Dapper;
using FluentAssertions;
using GlavLib.Basics.DataTypes;
using GlavLib.Db;
using GlavLib.Tests.Extensions;
using GlavLib.Tests.Model;

namespace GlavLib.Tests.Db;

/// <summary>
/// Пользовательские типы NHibernate на PostgreSQL. В отличие от SQLite, здесь у базы есть
/// собственные типы даты и времени, поэтому значения ложатся в `timestamptz` и `date`,
/// а не строкой.
/// </summary>
[Collection(nameof(IntegrationTestsCollection))]
public sealed class NpgsqlUserTypesTests : IClassFixture<NpgsqlTestDatabase>
{
    private readonly NpgsqlTestDatabase _database;

    public NpgsqlUserTypesTests(NpgsqlTestDatabase database)
    {
        _database = database;
    }

    private StatefulDbSession OpenSession()
    {
        return _database.SessionFactory.OpenStatefulSession(ConnectionStringNames.Master);
    }

    [Fact]
    public void It_should_assign_identifier_from_sequence()
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

    /// <summary>
    /// Проверяет, что типы кладут в колонки собственные значения PostgreSQL, а не строки:
    /// вставка идёт в `timestamptz` и `date`, а прочитанное сырым запросом значение —
    /// `DateTime`, а не текст.
    /// </summary>
    [Fact]
    public void It_should_store_values_in_native_column_types()
    {
        var createdAt = new UtcDateTime(2026, 9, 8, 14, 35, 12);

        using var dbSession = OpenSession();

        using (var transaction = new DbTransaction(dbSession))
        {
            dbSession.NhSession.Save(new StoredRecord
            {
                Name      = "native",
                CreatedAt = createdAt,
                BirthDate = new Date(1990, 5, 17),
                Period    = new YearMonth(2026, 1),
                Currency  = TestCurrency.Som
            });

            transaction.Commit();
        }

        var storedCreatedAt = dbSession.Connection.ExecuteScalar<DateTime>(
            "select created_at from stored_records where name = 'native'");

        var storedBirthDate = dbSession.Connection.ExecuteScalar<DateTime>(
            "select birth_date from stored_records where name = 'native'");

        var storedPeriod = dbSession.Connection.ExecuteScalar<DateTime>(
            "select period from stored_records where name = 'native'");

        var storedCurrency = dbSession.Connection.ExecuteScalar<string>(
            "select currency from stored_records where name = 'native'");

        storedCreatedAt.Should().Be(createdAt.Value);
        storedCreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        storedBirthDate.Should().Be(new DateTime(1990, 5, 17));
        storedPeriod.Should().Be(new DateTime(2026, 1, 1));
        storedCurrency.Should().Be("KGS");
    }

    [Fact]
    public void It_should_store_null_for_absent_values()
    {
        long id;

        using (var dbSession = OpenSession())
        using (var transaction = new DbTransaction(dbSession))
        {
            var record = new StoredRecord { Name = "nulls" };

            dbSession.NhSession.Save(record);
            transaction.Commit();

            id = record.Id;
        }

        using (var dbSession = OpenSession())
        {
            var record = dbSession.NhSession.Get<StoredRecord>(id);

            record.CreatedAt.Should().BeNull();
            record.BirthDate.Should().BeNull();
            record.Period.Should().BeNull();
            record.Duration.Should().BeNull();
            record.Payload.Should().BeNull();
            record.Currency.Should().BeNull();
        }
    }
}
