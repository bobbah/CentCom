using System.Collections.Generic;
using System.Text.Json.Serialization;
using CentCom.Server.External.Raw;

namespace CentCom.Server.External;

public class TgApiPagination
{
    [JsonPropertyName("items")]
    public int Items { get; set; }
    
    [JsonPropertyName("page")]
    public int Page { get; set; }
    
    [JsonPropertyName("per_page")]
    public int PerPage { get; set; }
}

public class TgApiResponse
{
    [JsonPropertyName("data")]
    public List<TgRawBan> Data { get; set; }
    
    [JsonPropertyName("pagination")]
    public TgApiPagination Pagination { get; set; }
}