using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Humanizer;

namespace GlavLib.App.Db.NhConventions;

public sealed class ClassConvention : IClassConvention
{
    public void Apply(IClassInstance instance)
    {
        instance.Table(instance.EntityType.Name.Underscore().Pluralize());
    }
}