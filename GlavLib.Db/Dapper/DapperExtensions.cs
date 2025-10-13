using System.Reflection;
using Dapper;
using GlavLib.Abstractions.DataTypes;

namespace GlavLib.Db.Dapper;

public static class DapperExtensions
{
    public static void AddEnumObjectTypeHandlers(Assembly assembly)
    {
        var types           = assembly.GetTypes();
        var enumObjectTypes = types.Where(t => t.IsSubclassOf(typeof(EnumObject)));

        foreach (var enumObjectType in enumObjectTypes)
        {
            var handlerType = typeof(EnumObjectTypeHandler<>).MakeGenericType(enumObjectType);
            var handler     = (SqlMapper.ITypeHandler)Activator.CreateInstance(handlerType)!;

            SqlMapper.AddTypeHandler(enumObjectType, handler);
        }
    }
}