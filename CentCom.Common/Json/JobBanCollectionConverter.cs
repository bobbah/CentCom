using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using CentCom.Common.Abstract;
using CentCom.Common.Models.Rest;

namespace CentCom.Common.Json;

/// <summary>
/// Converter to serialize and deserialize collections of job bans into arrays of strings
/// </summary>
public class JobBanCollectionConverter : JsonConverter<IReadOnlyList<IRestJobBan>>
{
    public override IReadOnlyList<IRestJobBan> Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected an array of job bans.");

        var toReturn = new List<IRestJobBan>();
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.EndArray:
                    return toReturn;
                case JsonTokenType.String:
                    toReturn.Add(new RestJobBan(reader.GetString()));
                    break;
                default:
                    throw new JsonException("Expected a non-null string job ban.");
            }
        }

        throw new JsonException("Unexpected end of job ban array.");
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyList<IRestJobBan> value,
        JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();
        foreach (var job in value)
        {
            var name = job?.Job;
            if (name == null)
                throw new JsonException("Job ban entries must be non-null strings.");
            writer.WriteStringValue(name);
        }

        writer.WriteEndArray();
    }
}