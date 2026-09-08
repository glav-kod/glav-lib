using JetBrains.Annotations;

namespace GlavLib.Db.Providers;

/// <summary>
/// Открывает сессии работы с базой данных. Конкретная СУБД выбирается наследником,
/// который регистрируется вызовом <c>AddNpgsql</c> либо <c>AddSqlite</c>.
/// </summary>
[PublicAPI]
public abstract class DbSessionFactory
{
    public abstract StatefulDbSession OpenStatefulSession(string connectionStringName);

    public abstract StatelessDbSession OpenStatelessSession(string connectionStringName);

    /// <summary>
    /// Закрывает все соединения, открытые фабрикой, и опустошает пул.
    /// </summary>
    public abstract void CloseAllConnections();
}
