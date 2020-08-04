using System.Net;

namespace System.Text.Json.Serialization
{
	/// From https://github.com/Macross-Software/core
	public class JsonIPAddressConverter : JsonConverter<IPAddress>
	{
		/// <inheritdoc/>
		public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException();

			try
			{
				return IPAddress.Parse(reader.GetString());
			}
			catch (Exception ex)
			{
				throw new JsonException("Unexpected value format, unable to parse IPAddress.", ex);
			}
		}

        public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
        {
			writer.WriteStringValue(value.ToString());
        }
    }
}
