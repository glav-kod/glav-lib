using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using GlavLib.Basics.DataTypes;

namespace GlavLib.Db.NhUserTypes;

/// <summary>
/// Двойник <see cref="NpgsqlUserTypesConventions"/> для SQLite: типов даты и времени в SQLite нет,
/// поэтому собственные типы хранятся текстом в каноническом виде, а не в том, который
/// выбрал бы за них диалект.
/// </summary>
internal sealed class SqliteUserTypesConventions : IPropertyConvention, IIdConvention
{
    public void Apply(IPropertyInstance instance)
    {
        if (instance.Property.PropertyType == typeof(UtcDateTime))
            instance.CustomType<SqliteUtcDateTimeUserType>();

        if (instance.Property.PropertyType == typeof(Date))
            instance.CustomType<SqliteDateUserType>();

        if (instance.Property.PropertyType == typeof(YearMonth))
            instance.CustomType<SqliteYearMonthUserType>();

        if (instance.Property.PropertyType == typeof(TimeSpan))
            instance.CustomType("TimeAsTimeSpan");
    }

    public void Apply(IIdentityInstance instance)
    {
        if (instance.Type == typeof(UtcDateTime))
            instance.CustomType<SqliteUtcDateTimeUserType>();

        if (instance.Type == typeof(Date))
            instance.CustomType<SqliteDateUserType>();

        if (instance.Type == typeof(YearMonth))
            instance.CustomType<SqliteYearMonthUserType>();
    }
}
