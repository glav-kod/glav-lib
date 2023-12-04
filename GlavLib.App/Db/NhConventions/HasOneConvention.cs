using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using FluentNHibernate.Mapping;

namespace GlavLib.App.Db.NhConventions;

public sealed class HasOneConvention : IHasOneConvention
{
    public void Apply(IOneToOneInstance instance)
    {
        instance.LazyLoad(Laziness.Proxy);
        instance.Cascade.SaveUpdate();
    }
}