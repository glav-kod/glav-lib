using Dapper;
using GlavLib.Basics.DataTypes;
using GlavLib.Db.Dapper;

namespace GlavLib.Sandbox.API.Db;

public static class DapperConventions
{
    public static void Setup()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        SqlMapper.AddTypeHandler(typeof(Date), new DateHandler());
        SqlMapper.AddTypeHandler(typeof(UtcDateTime), new UtcDateTimeHandler());
    }
}