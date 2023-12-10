using FluentNHibernate.Mapping;

namespace GlavLib.Sandbox.API.Model;

public class User
{
    public virtual long Id { get; protected set; }

    public virtual string Name { get; protected set; } = null!;

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

    public class Map : ClassMap<User>
    {
        public Map()
        {
            Id(x => x.Id);
            
            Map(x => x.Name);
        }
    }
}