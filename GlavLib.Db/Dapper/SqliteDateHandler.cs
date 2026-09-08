using System.Data;
using Dapper;
using GlavLib.Basics.DataTypes;

namespace GlavLib.Db.Dapper;

/// <inheritdoc cref="SqliteUtcDateTimeHandler"/>
public sealed class SqliteDateHandler : SqlMapper.TypeHandler<Date>
{
    public override void SetValue(
            IDbDataParameter parameter,
            Date? dateTime
        )
    {
        parameter.Value = dateTime?.ToString();
    }

    public override Date Parse(object value)
    {
        var str = (string)value;
        return Date.FromString(str);
    }
}
