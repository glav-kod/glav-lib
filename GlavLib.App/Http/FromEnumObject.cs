using GlavLib.Abstractions.DataTypes;
using GlavLib.Basics.DataTypes;
using JetBrains.Annotations;

namespace GlavLib.App.Http;

public class FromEnumObject<T> : IQueryArg
    where T : IEnumObject<T>
{
    public required T Value { get; init; }

    [PublicAPI]
    public static bool TryParse(string? value, out FromEnumObject<T>? result)
    {
        if (value is null)
        {
            result = null;
            return false;
        }

        result = new FromEnumObject<T>
        {
            Value = T.Create(value)
        };

        return true;
    }

    public object GetArgumentValue() => Value;
}