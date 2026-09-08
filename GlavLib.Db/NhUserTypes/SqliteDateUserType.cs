using GlavLib.Basics.DataTypes;
using NHibernate;
using NHibernate.Type;

namespace GlavLib.Db.NhUserTypes;

/// <summary>
/// Типа даты в SQLite нет, а формат, в котором её записал бы диалект NHibernate, —
/// его внутренняя деталь: <see cref="Date"/> уехал бы в базу как дата-время
/// <c>1990-05-17 00:00:00</c>, и прочитать колонку в обход NHibernate стало бы невозможно.
/// Поэтому значение хранится в каноническом виде самого типа — том, что даёт
/// <see cref="Date.ToString"/>.
/// </summary>
internal sealed class SqliteDateUserType : SingleValueObjectType<Date>
{
    protected override NullableType PrimitiveType => NHibernateUtil.String;

    protected override Date Create(object value)
    {
        return Date.FromString((string)value);
    }

    protected override object GetValue(Date state)
    {
        return state.ToString();
    }
}
