using GlavLib.Basics.DataTypes;
using NHibernate;
using NHibernate.Type;

namespace GlavLib.Db.NhUserTypes;

internal sealed class DateUserType : SingleValueObjectType<Date>
{
    protected override NullableType PrimitiveType => NHibernateUtil.Date;

    protected override Date Create(object value)
    {
        var dateTime = Convert.ToDateTime(value);

        return Date.FromDateTime(dateTime);
    }

    protected override object GetValue(Date state)
    {
        return state.Value;
    }
}