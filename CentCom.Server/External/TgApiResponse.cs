using CentCom.Server.External.Raw;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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
