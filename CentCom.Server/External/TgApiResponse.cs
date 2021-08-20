using System.Collections.Generic;
using System.Text.Json.Serialization;
using CentCom.Server.External.Raw;

namespace CentCom.Server.External
{
    public class TgApiResponse
    {
        [JsonPropertyName("beforeid")]
        public int BeforeId { get; set; }

        [JsonPropertyName("bans")]
        public IEnumerable<TgRawBan> Bans { get; set; }
    }
}
