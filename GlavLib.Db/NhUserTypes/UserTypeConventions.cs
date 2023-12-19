using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using GlavLib.Basics.DataTypes;

namespace GlavLib.Db.NhUserTypes;

internal sealed class UserTypesConventions : IPropertyConvention, IIdConvention
{
    public void Apply(IPropertyInstance instance)
    {
        if (instance.Property.PropertyType == typeof(UtcDateTime))
            instance.CustomType<UtcDateTimeUserType>();

        if (instance.Property.PropertyType == typeof(Date))
            instance.CustomType<DateUserType>();
    }

    public void Apply(IIdentityInstance instance)
    {
        if (instance.Type == typeof(UtcDateTime))
            instance.CustomType<UtcDateTimeUserType>();

        if (instance.Type == typeof(Date))
            instance.CustomType<DateUserType>();
    }
}