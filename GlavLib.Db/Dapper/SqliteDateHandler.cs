using System.Data;
using System.Globalization;
using Dapper;
using GlavLib.Basics.DataTypes;

namespace GlavLib.Db.Dapper;

/// <inheritdoc cref="SqliteUtcDateTimeHandler"/>
public sealed class SqliteDateHandler : SqlMapper.TypeHandler<Date>
{
    public override void SetValue(IDbDataParameter parameter,
                                  Date?            dateTime)
    {
        parameter.Value = dateTime?.Value;
    }

    public override Date Parse(object value)
    {
        var dateTime = Convert.ToDateTime(value, CultureInfo.InvariantCulture);

        return Date.FromDateTime(dateTime);
    }
}
