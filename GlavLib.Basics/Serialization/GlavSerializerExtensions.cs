﻿using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using GlavLib.Basics.DataTypes;

namespace GlavLib.Basics.Serialization;

public static class GlavSerializerExtensions
{
    public static JsonSerializerOptions UsingGlavOptions(this JsonSerializerOptions options)
    {
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.WriteIndented = false;
        options.PropertyNameCaseInsensitive = true;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

        options.TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { ShouldSerializeOptional }
        };

        return options;
    }

    public static JsonSerializerOptions UsingGlavConverters(this JsonSerializerOptions options)
    {
        options.Converters.Add(new TimeSpanJsonConverter());
        options.Converters.Add(new ObjectJsonConverter());
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        options.Converters.Add(new UtcDateTimeJsonConverter());
        options.Converters.Add(new DateJsonConverter());
        options.Converters.Add(new YearMonthJsonConverter());
        options.Converters.Add(new EnumObjectJsonConverter());
        options.Converters.Add(new TimeZoneInfoJsonConverter());
        options.Converters.Add(new OptionalJsonConverter());

        return options;
    }

    public static JsonSerializerOptions GlavConfiguration(this JsonSerializerOptions options)
    {
        options.UsingGlavOptions()
               .UsingGlavConverters();

        return options;
    }
    
    private static void ShouldSerializeOptional(JsonTypeInfo jsonTypeInfo)
    {
        foreach (var jsonPropertyInfo in jsonTypeInfo.Properties)
        {
            if (!jsonPropertyInfo.PropertyType.IsAssignableTo(typeof(IOptional)))
                continue;
            
            jsonPropertyInfo.ShouldSerialize = (_, propValue) =>
            {
                if (propValue is null)
                    return false;
                
                var opt = (IOptional)propValue;
                return !opt.IsUndefined;
            };
        }
    }
}