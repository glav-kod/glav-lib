using FluentNHibernate.Mapping;
using GlavLib.Abstractions.Db;
using GlavLib.Basics.DataTypes;
using JetBrains.Annotations;

namespace GlavLib.Sandbox.API.Model;

public class User : Entity
{
    public virtual string Name { get; protected set; } = null!;

    public virtual Date? BirthDate { get; protected set; }

    protected User()
    {
    }

    public static User Create(string name)
    {
        return new User
        {
            Name = name
        };
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class NhClassMap : ClassMap<User>
    {
        public NhClassMap()
        {
            Id(x => x.Id);

            Map(x => x.Name);

            Map(x => x.BirthDate);
        }
    }
}
