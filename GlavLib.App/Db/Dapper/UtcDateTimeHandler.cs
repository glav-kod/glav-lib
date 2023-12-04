using System.Data;
using Dapper;
using GlavLib.Basics.DataTypes;

namespace GlavLib.App.Db.Dapper;

public sealed class UtcDateTimeHandler : SqlMapper.TypeHandler<UtcDateTime>
{
    public override void SetValue(IDbDataParameter parameter,
                                  UtcDateTime?     dateTime)
    {
        parameter.Value = dateTime?.Value;
    }

    public override UtcDateTime Parse(object value)
    {
        var dateTime = (DateTime)value;

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