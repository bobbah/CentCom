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
        var toReturn = new List<IRestJobBan>();
        do
        {
            if (reader.TokenType == JsonTokenType.StartArray)
                continue;
            if (reader.TokenType == JsonTokenType.EndArray || reader.TokenType == JsonTokenType.Null)
                break;
            RestJobBan job = reader.GetString();
            if (job != null)
                toReturn.Add(job);
        } while (reader.Read());

        return toReturn;
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
            writer.WriteStringValue(job.Job);
        }

        writer.WriteEndArray();
    }
}