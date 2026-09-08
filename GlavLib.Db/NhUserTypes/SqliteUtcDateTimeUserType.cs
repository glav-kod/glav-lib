using GlavLib.Basics.DataTypes;
using NHibernate;
using NHibernate.Type;

namespace GlavLib.Db.NhUserTypes;

/// <inheritdoc cref="SqliteDateUserType"/>
/// <remarks>
/// Канонический вид <see cref="UtcDateTime"/> — <c>yyyy-MM-ddTHH:mm:ssZ</c>, поэтому
/// на SQLite момент времени хранится с точностью до секунды.
/// </remarks>
internal sealed class SqliteUtcDateTimeUserType : SingleValueObjectType<UtcDateTime>
{
    protected override NullableType PrimitiveType => NHibernateUtil.String;

    protected override UtcDateTime Create(object value)
    {
        return UtcDateTime.ParseExact((string)value);
    }

    protected override object GetValue(UtcDateTime state)
    {
        return state.ToString();
    }
}
