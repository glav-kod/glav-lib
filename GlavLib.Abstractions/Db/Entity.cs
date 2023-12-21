namespace GlavLib.Abstractions.Db;

public abstract class Entity<TId>
{
    public virtual TId Id { get; protected set; } = default!;
}

public abstract class Entity : Entity<long>;