using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Humanizer;

namespace GlavLib.Db.NhConventions;

/// <summary>
/// Идентификаторы для SQLite. Отличается от <see cref="NpgsqlIdConvention"/> генератором:
/// секвенций в SQLite нет, идентификатор выдаёт сама база при вставке.
/// </summary>
public sealed class SqliteIdConvention : IIdConvention
{
    public void Apply(IIdentityInstance instance)
    {
        if (string.IsNullOrEmpty(instance.GeneratedBy.Class))
        {
            instance.GeneratedBy.Identity();

            instance.UnsavedValue("0");
        }

        if (instance.Name is not null)
            instance.Column(instance.Name.Underscore());
    }
}
