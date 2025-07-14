using GlavLib.Basics.DataTypes;
using NHibernate;
using NHibernate.Type;

namespace GlavLib.Db.NhUserTypes;

internal sealed class YearMonthUserType : SingleValueObjectType<YearMonth>
{
    protected override NullableType PrimitiveType => NHibernateUtil.Date;

    protected override YearMonth Create(object value)
    {
        var dateTime = Convert.ToDateTime(value);

        return YearMonth.FromDateTime(dateTime);
    }

    protected override object GetValue(YearMonth state)
    {
        return state.Value;
    }
}