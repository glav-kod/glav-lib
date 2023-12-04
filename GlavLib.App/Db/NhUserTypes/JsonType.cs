using System.Data.Common;
using System.Text.Json;
using GlavLib.Basics.Extensions;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace GlavLib.App.Db.NhUserTypes;

public class JsonType<T> : IUserType where T : class
{
    public Type ReturnedType => typeof(T);

    protected virtual JsonSerializerOptions Options => GlavJsonSerializer.Options;

    public object Assemble(object cached, object owner)
    {
        return cached;
    }

    public object? DeepCopy(object? value)
    {
        if (value is null)
            return null;

        var json = Serialize((T)value);

        return Deserialize(json);
    }

    public object Disassemble(object value)
    {
        return value;
    }

    public new bool Equals(object? x, object? y)
    {
        var left  = x as T;
        var right = y as T;

        if (left == null && right == null)
            return true;

        if (left == null || right == null)
            return false;

        return Serialize(left).Equals(Serialize(right));
    }

    public int GetHashCode(object x)
    {
        return x.GetHashCode();
    }

    public bool IsMutable => true;

    public object? NullSafeGet(DbDataReader rs,
                               string[] names,
                               ISessionImplementor session,
                               object? owner)
    {
        var returnValue = NHibernateUtil.String.NullSafeGet(rs, names[0], session, owner);
        if (returnValue == null)
            return null;

        var json = returnValue.ToString()!;

        return Deserialize(json);
    }

    public void NullSafeSet(DbCommand cmd,
                            object? value,
                            int index,
                            ISessionImplementor session)
    {
        if (value is not T column)
        {
            NHibernateUtil.String.NullSafeSet(cmd, null, index, session);
            return;
        }

        value = Serialize(column);
        NHibernateUtil.String.NullSafeSet(cmd, value, index, session);
    }

    public object Replace(object original, object target, object owner)
    {
        return original;
    }

    public SqlType[] SqlTypes { get; } = { new StringSqlType() };

    protected virtual string Serialize(T obj)
    {
        return JsonSerializer.Serialize(obj, Options);
    }

    protected virtual T Deserialize(string json)
    {
        return JsonSerializer.Deserialize<T>(json, Options)!;
    }
}