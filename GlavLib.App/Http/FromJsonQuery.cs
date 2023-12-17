using System.Text.Json;
using GlavLib.Basics.Serialization;
using JetBrains.Annotations;

namespace GlavLib.App.Http;

public static class FromJsonQuery
{
    public static JsonSerializerOptions JsonOptions { get; set; } = GlavJsonSerializer.Options;
}

public class FromJsonQuery<T> : IQueryArg
{
    public required T Value { get; init; }

    [PublicAPI]
    public static bool TryParse(string? value, out FromJsonQuery<T>? result)
    {
        var obj = value is not null
            ? JsonSerializer.Deserialize<T>(value, FromJsonQuery.JsonOptions)
            : default;
        
        result = new FromJsonQuery<T>
        {
            Value = obj!
        };
        return true;
    }

    public object? GetArgumentValue() => Value;
}