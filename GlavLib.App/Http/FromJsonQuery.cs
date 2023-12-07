using GlavLib.Basics.Serialization;
using JetBrains.Annotations;

namespace GlavLib.App.Http;

public class FromJsonQuery<T> : IQueryArg
{
    public required T Value { get; init; }

    [PublicAPI]
    public static bool TryParse(string? value, out FromJsonQuery<T>? result)
    {
        result = new FromJsonQuery<T>
        {
            Value = GlavJsonSerializer.Deserialize<T>(value)!
        };
        return true;
    }

    public object? GetArgumentValue() => Value;
}