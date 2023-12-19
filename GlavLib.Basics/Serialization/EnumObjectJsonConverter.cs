using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using GlavLib.Abstractions.DataTypes;

namespace GlavLib.Basics.Serialization;

public class EnumObjectJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(EnumObject).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var enumObjectConverter = typeof(EnumObjectJsonConverter<>).MakeGenericType(typeToConvert);

        return (JsonConverter)Activator.CreateInstance(enumObjectConverter)!;
    }
}

internal sealed class EnumObjectJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : EnumObject
{
    private static Func<string, TEnum>? _enumFactory;

    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(TEnum).IsAssignableFrom(typeToConvert);
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var key = reader.GetString()!;

        var enumFactory = GetEnumFactory();
        return enumFactory(key);
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Key);
    }

    private static Func<string, TEnum> GetEnumFactory()
    {
        if (_enumFactory is null)
        {
            var methodInfo = typeof(TEnum).GetMethod("Create", BindingFlags.Static | BindingFlags.Public);
            if (methodInfo is null)
                throw new InvalidOperationException($"Cannot create JsonConverter, '{typeof(TEnum)}' doesn't contain static 'Create' method");

            _enumFactory = methodInfo.CreateDelegate<Func<string, TEnum>>();
        }

        return _enumFactory;
    }
}