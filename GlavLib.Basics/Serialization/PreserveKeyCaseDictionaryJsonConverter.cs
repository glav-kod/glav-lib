using System.Text.Json;
using System.Text.Json.Serialization;

namespace GlavLib.Basics.Serialization;

/// <summary>
/// Enables reading/writing a dictionary as JSON.
/// Implemented to support reading/writing non-standard keys.
/// </summary>
public sealed class PreserveKeyCaseDictionaryJsonConverter : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;

        var definition = typeToConvert.GetGenericTypeDefinition();

        var genericArguments = typeToConvert.GetGenericArguments();
        
        return (definition == typeof(IDictionary<,>)
                || definition == typeof(Dictionary<,>))
               && genericArguments[0] == typeof(string);
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (!CanConvert(typeToConvert))
        {
            throw new InvalidOperationException($"{typeToConvert} is not a valid type for converter {typeof(DictionaryJsonConverter<,>)}");
        }

        var genericArguments = typeToConvert.GetGenericArguments();
        var converterType    = typeof(PreserveKeyCaseDictionaryJsonConverter<>).MakeGenericType(genericArguments[1]);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public class PreserveKeyCaseDictionaryJsonConverter<TValue> : JsonConverter<IDictionary<string, TValue>>
{
    /// <inheritdoc />
    public override IDictionary<string, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Unexpected JsonToken '{reader.TokenType}' in converter {GetType()}.");

        // step forward
        reader.Read();

        var instance = new Dictionary<string, TValue>();
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            var (key, value) = ReadEntry(ref reader, options);
            instance.Add(key, value);
        }

        return instance;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IDictionary<string, TValue> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var (key, val) in value)
        {
            writer.WritePropertyName(key);
            JsonSerializer.Serialize(writer, val, options);
        }

        writer.WriteEndObject();
    }

    private (string Key, TValue Value) ReadEntry(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException($"Unexpected JsonToken '{reader.TokenType}' in converter {GetType()}.");
        }

        var key = reader.GetString();

        reader.Read();

        var value = JsonSerializer.Deserialize<TValue>(ref reader, options);

        reader.Read();

        return (key!, value!);
    }
}