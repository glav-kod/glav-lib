using System.Data;
using System.Globalization;
using Dapper;
using GlavLib.Basics.DataTypes;

namespace GlavLib.Db.Dapper;

/// <summary>
/// SQLite хранит момент времени текстом в ISO-8601, и Dapper получает из такой колонки
/// строку, а не <see cref="DateTime"/>, поэтому значение разбирается, а не приводится.
/// </summary>
public sealed class SqliteUtcDateTimeHandler : SqlMapper.TypeHandler<UtcDateTime>
{
    public override void SetValue(IDbDataParameter parameter,
                                  UtcDateTime?     dateTime)
    {
        parameter.Value = dateTime?.Value;
    }

    public override UtcDateTime Parse(object value)
    {
        var dateTime = Convert.ToDateTime(value, CultureInfo.InvariantCulture);

        var utcDateTime = new DateTime(dateTime.Year,
                                       dateTime.Month,
                                       dateTime.Day,
                                       dateTime.Hour,
                                       dateTime.Minute,
                                       dateTime.Second,
                                       dateTime.Millisecond,
                                       DateTimeKind.Utc);

        return UtcDateTime.FromDateTime(utcDateTime);
    }
}
