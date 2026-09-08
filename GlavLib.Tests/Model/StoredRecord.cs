using FluentNHibernate.Mapping;
using GlavLib.Abstractions.DataTypes;
using GlavLib.Abstractions.Db;
using GlavLib.Basics.DataTypes;
using GlavLib.Db.Extensions;
using GlavLib.Db.NhUserTypes;
using JetBrains.Annotations;

namespace GlavLib.Tests.Model;

[EnumObjectItem("Som", "KGS", "Сом")]
[EnumObjectItem("Tenge", "KZT", "Тенге")]
public sealed partial class TestCurrency : EnumObject;

public sealed class TestPayload
{
    public string Title { get; set; } = null!;

    public int Count { get; set; }
}

/// <summary>
/// Сущность, собирающая в одной таблице всё, что библиотека обещает поддерживать:
/// собственные типы дат, JSON-колонку и <see cref="EnumObject"/>. Маппинг один на обе СУБД —
/// именно это и проверяют наборы тестов PostgreSQL и SQLite.
/// </summary>
public class StoredRecord : Entity
{
    public virtual string Name { get; set; } = null!;

    public virtual UtcDateTime? CreatedAt { get; set; }

    public virtual Date? BirthDate { get; set; }

    public virtual YearMonth? Period { get; set; }

    public virtual TimeSpan? Duration { get; set; }

    public virtual TestPayload? Payload { get; set; }

    public virtual TestCurrency? Currency { get; set; }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class NhClassMap : ClassMap<StoredRecord>
    {
        public NhClassMap()
        {
            Id(x => x.Id);

            Map(x => x.Name);
            Map(x => x.CreatedAt);
            Map(x => x.BirthDate);
            Map(x => x.Period);
            Map(x => x.Duration);
            Map(x => x.Payload).CustomType<JsonType<TestPayload>>();
            Map(x => x.Currency).EnumObjectType<TestCurrency>();
        }
    }
}
