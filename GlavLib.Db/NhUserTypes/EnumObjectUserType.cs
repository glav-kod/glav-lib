using GlavLib.Abstractions.DataTypes;
using NHibernate;
using NHibernate.Type;

namespace GlavLib.Db.NhUserTypes;

public sealed class EnumObjectUserType<TValue> : SingleValueObjectType<TValue>
    where TValue : class, IEnumObject<TValue>
{
    protected override NullableType PrimitiveType => NHibernateUtil.String;

    protected override TValue Create(object value)
    {
        var key = Convert.ToString(value)!;

        return TValue.Create(key);
    }

    protected override object GetValue(TValue state)
    {
        return state.Key;
    }
}