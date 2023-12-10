using Dapper;
using GlavLib.App.Db.Dapper;
using GlavLib.Basics.DataTypes;

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