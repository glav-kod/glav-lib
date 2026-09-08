using Microsoft.Extensions.Configuration;

namespace GlavLib.Db.Providers;

internal static class ConnectionStringResolver
{
    /// <summary>
    /// Возвращает строку подключения по её имени. Отсутствующее имя — ошибка конфигурации
    /// приложения, поэтому сообщение называет искомое имя: иначе ошибка всплывает
    /// внутри драйвера базы данных и не подсказывает, чего не хватает.
    /// </summary>
    public static string Resolve(IConfiguration configuration, string connectionStringName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Connection string '{connectionStringName}' is not found in configuration");

        return connectionString;
    }
}
