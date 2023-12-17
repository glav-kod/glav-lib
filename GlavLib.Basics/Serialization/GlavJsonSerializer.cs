using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace GlavLib.Basics.Serialization;

public static class GlavJsonSerializer
{
    public static readonly JsonSerializerOptions Options = new JsonSerializerOptions()
        .GlavConfiguration();

    [return: NotNullIfNotNull("value")]
    public static string? Serialize<T>(T? value)
    {
        if (ReferenceEquals(value, null))
            return default;

        return JsonSerializer.Serialize(value, typeof(T), Options);
    }

    [return: NotNullIfNotNull("json")]
    public static T? Deserialize<T>(string? json)
    {
        return json is not null
            ? JsonSerializer.Deserialize<T>(json, Options)
            : default;
    }

    [return: NotNullIfNotNull("json")]
    public static object? Deserialize(string? json, Type type)
    {
        return json is not null
            ? JsonSerializer.Deserialize(json, type, Options)
            : default;
    }
}