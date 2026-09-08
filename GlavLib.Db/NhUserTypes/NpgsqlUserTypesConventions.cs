using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using GlavLib.Basics.DataTypes;

namespace GlavLib.Db.NhUserTypes;

internal sealed class NpgsqlUserTypesConventions : IPropertyConvention, IIdConvention
{
    public void Apply(IPropertyInstance instance)
    {
        if (instance.Property.PropertyType == typeof(UtcDateTime))
            instance.CustomType<NpgsqlUtcDateTimeUserType>();

        if (instance.Property.PropertyType == typeof(Date))
            instance.CustomType<NpgsqlDateUserType>();

        if (instance.Property.PropertyType == typeof(YearMonth))
            instance.CustomType<NpgsqlYearMonthUserType>();

        if (instance.Property.PropertyType == typeof(TimeSpan))
            instance.CustomType("TimeAsTimeSpan");
    }

    public void Apply(IIdentityInstance instance)
    {
        if (instance.Type == typeof(UtcDateTime))
            instance.CustomType<NpgsqlUtcDateTimeUserType>();

        if (instance.Type == typeof(Date))
            instance.CustomType<NpgsqlDateUserType>();

        if (instance.Type == typeof(YearMonth))
            instance.CustomType<NpgsqlYearMonthUserType>();
    }
}