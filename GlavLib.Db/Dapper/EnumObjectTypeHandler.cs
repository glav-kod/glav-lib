using System.Data;
using Dapper;
using GlavLib.Abstractions.DataTypes;

namespace GlavLib.Db.Dapper;

public sealed class EnumObjectTypeHandler<TEnumObject> : SqlMapper.ITypeHandler
    where TEnumObject : IEnumObject<TEnumObject>

{
    public void SetValue(IDbDataParameter parameter, object value)
    {
        var enumObject = (TEnumObject)value;

        parameter.Value = enumObject.Key;
    }

    public object Parse(Type destinationType, object value)
    {
        var key = value.ToString()!;

        return TEnumObject.Create(key);
    }
}
