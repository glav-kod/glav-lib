using System.Text.Json;
using System.Text.Json.Serialization;

namespace GlavLib.Basics.Serialization;

/// <summary>
/// Enables reading/writing a dictionary as JSON.
/// Implemented to support reading/writing non-standard keys.
/// </summary>
public sealed class DictionaryJsonConverter : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;

        var definition = typeToConvert.GetGenericTypeDefinition();

        return (definition == typeof(IDictionary<,>)
                || definition == typeof(Dictionary<,>))
               && typeToConvert.GetGenericArguments()[0] != typeof(string);
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (!CanConvert(typeToConvert))
        {
            throw new InvalidOperationException($"{typeToConvert} is not a valid type for converter {typeof(DictionaryJsonConverter<,>)}");
        }

        var converterType = typeof(DictionaryJsonConverter<,>).MakeGenericType(typeToConvert.GetGenericArguments());
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public class DictionaryJsonConverter<TKey, TValue> : JsonConverter<IDictionary<TKey, TValue>>
    where TKey : notnull
{
    /// <inheritdoc />
    public override IDictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Unexpected JsonToken '{reader.TokenType}' in converter {GetType()}.");

        // step forward
        reader.Read();

        var instance = new Dictionary<TKey, TValue>();
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            var (key, value) = ReadEntry(ref reader, options);
            instance.Add(key, value);
        }

        return instance;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IDictionary<TKey, TValue> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var (key, val) in value)
        {
            writer.WritePropertyName(JsonSerializer.Serialize(key, options));
            JsonSerializer.Serialize(writer, val, options);
        }

        writer.WriteEndObject();
    }

    private (TKey Key, TValue Value) ReadEntry(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException($"Unexpected JsonToken '{reader.TokenType}' in converter {GetType()}.");
        }

        var key = JsonSerializer.Deserialize<TKey>(reader.ValueSpan, options);

        reader.Read();

        var value = JsonSerializer.Deserialize<TValue>(ref reader, options);

        reader.Read();

        return (key!, value!);
    }
}