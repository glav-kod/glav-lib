using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using NHibernate.Type;

namespace GlavLib.App.Db.NhConventions;

public sealed class EnumConvention : IUserTypeConvention
{
    public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
    {
        criteria.Expect(x => x.Property.PropertyType.IsEnum);
    }

    public void Apply(IPropertyInstance target)
    {
        var type       = typeof(EnumType<>);
        var customType = type.MakeGenericType(target.Property.PropertyType);

        target.CustomType(customType);
    }
}
