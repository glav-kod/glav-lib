using System.Text.Json;
using System.Text.Json.Serialization;
using GlavLib.Basics.DataTypes;

namespace GlavLib.Basics.Serialization;

public sealed class DateJsonConverter : JsonConverter<Date>
{
    public override Date Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString()!;
        return (Date)value;
    }

    public override void Write(Utf8JsonWriter writer, Date value, JsonSerializerOptions options)
    {
        var stringDateTime = value.ToString();
        writer.WriteStringValue(stringDateTime);
    }
}