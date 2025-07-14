using System.Data;
using Dapper;
using GlavLib.Basics.DataTypes;

namespace GlavLib.Db.Dapper;

public sealed class YearMonthHandler : SqlMapper.TypeHandler<YearMonth>
{
    public override void SetValue(
            IDbDataParameter parameter,
            YearMonth? dateTime
        )
    {
        parameter.Value = dateTime?.Value;
    }

    public override YearMonth Parse(object value)
    {
        var dateTime = (DateTime)value;

        return YearMonth.FromDateTime(dateTime);
    }
}