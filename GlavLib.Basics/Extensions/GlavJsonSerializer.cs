using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace GlavLib.Basics.Extensions;

public static class GlavJsonSerializer
{
    public static readonly JsonSerializerOptions Options = new JsonSerializerOptions()
        .ConfigureSystem();

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
        if (ReferenceEquals(json, null))
            return default;

        return JsonSerializer.Deserialize<T>(json, Options)!;
    }

    [return: NotNullIfNotNull("json")]
    public static object? Deserialize(string? json, Type type)
    {
        if (ReferenceEquals(json, null))
            return default;

        return JsonSerializer.Deserialize(json, type, Options)!;
    }
}