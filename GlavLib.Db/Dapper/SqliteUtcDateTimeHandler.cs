using System.Data;
using Dapper;
using GlavLib.Basics.DataTypes;

namespace GlavLib.Db.Dapper;

/// <summary>
/// На SQLite собственные типы дат хранятся текстом в каноническом виде самого типа,
/// поэтому Dapper читает и пишет их тем же <c>ToString</c>/<c>Parse</c>, что и
/// пользовательские типы NHibernate: формат задаёт тип, а не диалект.
/// </summary>
public sealed class SqliteUtcDateTimeHandler : SqlMapper.TypeHandler<UtcDateTime>
{
    public override void SetValue(
            IDbDataParameter parameter,
            UtcDateTime? dateTime
        )
    {
        parameter.Value = dateTime?.ToString();
    }

    public override UtcDateTime Parse(object value)
    {
        return UtcDateTime.ParseExact((string)value);
    }
}
