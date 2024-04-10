using System.Text.Json;
using System.Text.Json.Serialization;
using GlavLib.Basics.DataTypes;

namespace GlavLib.Basics.Serialization;

public sealed class OptionalJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;
        
        return typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var tOptional = typeToConvert.GenericTypeArguments[0];

        var optionalConverter = typeof(OptionalJsonConverter<>).MakeGenericType(tOptional);

        return (JsonConverter)Activator.CreateInstance(optionalConverter)!;
    }
}


public sealed class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
    public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return Optional<T>.Null;

        return JsonSerializer.Deserialize<T>(ref reader, options)!;
    }

    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        JsonSerializer.Serialize(writer, value.Value, options);
    }
}