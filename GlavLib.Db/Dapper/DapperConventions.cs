using Dapper;
using GlavLib.Basics.DataTypes;
using JetBrains.Annotations;

namespace GlavLib.Db.Dapper;

/// <summary>
/// Настраивает Dapper под выбранную СУБД. Набор обработчиков зависит от того, в каком виде
/// база отдаёт даты, поэтому СУБД названа прямо в имени метода: перепутать набор — значит
/// получить ошибку при первом же чтении даты.
/// </summary>
/// <remarks>
/// Настройки Dapper глобальны для процесса, поэтому вызывать нужно один раз при старте
/// приложения, а не при регистрации сервисов.
/// </remarks>
[PublicAPI]
public static class DapperConventions
{
    public static void SetupNpgsql()
    {
        SetupCommon();

        SqlMapper.AddTypeHandler(typeof(Date), new DateHandler());
        SqlMapper.AddTypeHandler(typeof(UtcDateTime), new UtcDateTimeHandler());
        SqlMapper.AddTypeHandler(typeof(YearMonth), new YearMonthHandler());
    }

    public static void SetupSqlite()
    {
        SetupCommon();

        SqlMapper.AddTypeHandler(typeof(Date), new SqliteDateHandler());
        SqlMapper.AddTypeHandler(typeof(UtcDateTime), new SqliteUtcDateTimeHandler());
        SqlMapper.AddTypeHandler(typeof(YearMonth), new SqliteYearMonthHandler());
    }

    private static void SetupCommon()
    {
        //Конвенции библиотеки безусловно режут имена таблиц и колонок в snake_case,
        //поэтому Dapper приводится к тому же виду.
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }
}
