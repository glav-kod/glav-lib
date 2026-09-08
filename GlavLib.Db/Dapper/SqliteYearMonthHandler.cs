using System.Data;
using System.Globalization;
using Dapper;
using GlavLib.Basics.DataTypes;

namespace GlavLib.Db.Dapper;

/// <inheritdoc cref="SqliteUtcDateTimeHandler"/>
public sealed class SqliteYearMonthHandler : SqlMapper.TypeHandler<YearMonth>
{
    public override void SetValue(IDbDataParameter parameter,
                                  YearMonth?       dateTime)
    {
        parameter.Value = dateTime?.Value;
    }

    public override YearMonth Parse(object value)
    {
        var dateTime = Convert.ToDateTime(value, CultureInfo.InvariantCulture);

        return YearMonth.FromDateTime(dateTime);
    }
}
