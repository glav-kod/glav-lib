using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.Type;
using NHibernate.UserTypes;

namespace GlavLib.Db.NhUserTypes;

public abstract class SingleValueObjectType<TValue> : IUserType where TValue : class
{
    public SqlType[] SqlTypes => new[] {PrimitiveType.SqlType};

    public Type ReturnedType => typeof(TValue);

    public bool IsMutable => false;

    public object? Assemble(object? cached, object? owner) => cached;

    public object? DeepCopy(object? value) => value;

    public object? Disassemble(object? value) => value;

    public new bool Equals(object? x, object? y) => x?.Equals(y) ?? y?.Equals(x) ?? true;

    public int GetHashCode(object? x) => x?.GetHashCode() ?? 0;

    public object? NullSafeGet(DbDataReader rs,
                               string[] names,
                               ISessionImplementor session,
                               object? owner)
    {
        var value = PrimitiveType.NullSafeGet(rs, names[0], session, owner);
        if (value == null)
            return null;

        return Create(value);
    }

    public void NullSafeSet(DbCommand cmd,
                            object? value,
                            int index,
                            ISessionImplementor session)
    {
        cmd.Parameters[index].Value = value == null
            ? DBNull.Value
            : GetValue((TValue) value);
    }

    public object Replace(object original, object target, object owner)
    {
        return original;
    }

    protected abstract NullableType PrimitiveType { get; }

    protected abstract TValue Create(object value);

    protected abstract object GetValue(TValue valueObject);
}