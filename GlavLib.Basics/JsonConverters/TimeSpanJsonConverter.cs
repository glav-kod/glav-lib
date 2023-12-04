using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GlavLib.Basics.JsonConverters;

public sealed class TimeSpanJsonConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException();

        var str = reader.GetString()!;
        return TimeSpan.Parse(str, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        var str = value.ToString("c");
        writer.WriteStringValue(str);
    }
}