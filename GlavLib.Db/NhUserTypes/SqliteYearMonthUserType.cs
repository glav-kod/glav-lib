using GlavLib.Basics.DataTypes;
using NHibernate;
using NHibernate.Type;

namespace GlavLib.Db.NhUserTypes;

/// <inheritdoc cref="SqliteDateUserType"/>
internal sealed class SqliteYearMonthUserType : SingleValueObjectType<YearMonth>
{
    protected override NullableType PrimitiveType => NHibernateUtil.String;

    protected override YearMonth Create(object value)
    {
        var str = (string)value;

        var result = YearMonth.FromString(str);
        if (result.IsFailure)
            throw new InvalidOperationException($"Wrong value format: {str}");

        return result.Value;
    }

    protected override object GetValue(YearMonth state)
    {
        return state.ToString();
    }
}
