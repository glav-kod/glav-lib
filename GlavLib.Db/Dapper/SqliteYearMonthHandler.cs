using System.Data;
using Dapper;
using GlavLib.Basics.DataTypes;

namespace GlavLib.Db.Dapper;

/// <inheritdoc cref="SqliteUtcDateTimeHandler"/>
public sealed class SqliteYearMonthHandler : SqlMapper.TypeHandler<YearMonth>
{
    public override void SetValue(
            IDbDataParameter parameter,
            YearMonth? yearMonth
        )
    {
        parameter.Value = yearMonth?.ToString();
    }

    public override YearMonth Parse(object value)
    {
        var str = (string)value;

        var result = YearMonth.FromString(str);
        if (result.IsFailure)
            throw new InvalidOperationException($"Wrong value format: {str}");

        return result.Value;
    }
}
