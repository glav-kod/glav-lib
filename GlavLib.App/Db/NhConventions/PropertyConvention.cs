using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Humanizer;

namespace GlavLib.App.Db.NhConventions;

public sealed class PropertyConvention : IPropertyConvention
{
    public void Apply(IPropertyInstance instance)
    {
        instance.Column(instance.Name.Underscore());
    }
}