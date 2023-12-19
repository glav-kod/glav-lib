using GlavLib.Basics.DataTypes;
using NHibernate;
using NHibernate.Type;

namespace GlavLib.Db.NhUserTypes;

internal sealed class UtcDateTimeUserType : SingleValueObjectType<UtcDateTime>
{
    protected override NullableType PrimitiveType => NHibernateUtil.UtcDateTime;

    protected override UtcDateTime Create(object value)
    {
        var dateTime = Convert.ToDateTime(value);
        return UtcDateTime.FromDateTime(dateTime);
    }

    protected override object GetValue(UtcDateTime state)
    {
        return state.Value;
    }
}