using System.Data;
using Dapper;
using GlavLib.Basics.DataTypes;

namespace GlavLib.App.Db.Dapper;

public sealed class DateHandler : SqlMapper.TypeHandler<Date>
{
    public override void SetValue(IDbDataParameter parameter,
                                  Date?            dateTime)
    {
        parameter.Value = dateTime?.Value;
    }

    public override Date Parse(object value)
    {
        var dateTime = (DateTime)value;

        return Date.FromDateTime(dateTime);
    }
}