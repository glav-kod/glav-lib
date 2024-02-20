using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using GlavLib.Basics.DataTypes;

namespace GlavLib.Basics.Serialization;

public sealed class UtcDateTimeJsonConverter : JsonConverter<UtcDateTime>
{
    public override UtcDateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString()!;

        var dateTime = DateTime.ParseExact("yyyy-MM-ddTHH:mm:ssK", value, CultureInfo.InvariantCulture);

        return UtcDateTime.FromDateTime(dateTime);
    }

    public override void Write(Utf8JsonWriter writer, UtcDateTime value, JsonSerializerOptions options)
    {
        var stringDateTime = value.ToString();

        writer.WriteStringValue(stringDateTime);
    }
}