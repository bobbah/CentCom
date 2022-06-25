using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using CentCom.Common.Abstract;
using CentCom.Common.Models.Byond;

namespace CentCom.Common.Json;

/// <summary>
/// Converter to serialize and deserialize ckeys as strings
/// </summary>
public class CKeyStringConverter : JsonConverter<ICKey>
{
    public override ICKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return (CKey)reader.GetString();
    }

    public override void Write(Utf8JsonWriter writer, ICKey value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.CanonicalKey);
    }
}