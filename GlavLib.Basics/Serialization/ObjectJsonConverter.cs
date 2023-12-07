using System.Text.Json;
using System.Text.Json.Serialization;

namespace GlavLib.Basics.Serialization;

public sealed class ObjectJsonConverter : JsonConverter<object?>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;

            case JsonTokenType.StartObject:
                return JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options);

            case JsonTokenType.StartArray:
                return JsonSerializer.Deserialize<List<object>>(ref reader, options);

            case JsonTokenType.String:
                return reader.GetString();

            case JsonTokenType.True:
                return true;

            case JsonTokenType.False:
                return false;

            case JsonTokenType.Number:
                return reader.GetSingle();

            default:
                throw new InvalidOperationException($"Unknown TokenType '{reader.TokenType}'");
        }
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        var valueType = value.GetType();

        if (valueType == typeof(object))
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
            return;
        }

        switch (value)
        {
            case long @long:
                writer.WriteNumberValue(@long);
                return;

            case ulong @ulong:
                writer.WriteNumberValue(@ulong);
                return;

            case int @int:
                writer.WriteNumberValue(@int);
                return;

            case uint @uint:
                writer.WriteNumberValue(@uint);
                return;

            case float @float:
                writer.WriteNumberValue(@float);
                return;

            case double @double:
                writer.WriteNumberValue(@double);
                return;

            case decimal @decimal:
                writer.WriteNumberValue(@decimal);
                return;

            case bool @bool:
                writer.WriteBooleanValue(@bool);
                return;

            case string @string:
                writer.WriteStringValue(@string);
                return;
        }

        JsonSerializer.Serialize(writer, value, valueType, options);
    }
}