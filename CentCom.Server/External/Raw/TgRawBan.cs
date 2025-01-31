using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CentCom.Common.Extensions;
using CentCom.Common.Models;

namespace CentCom.Server.External.Raw;

public class TgUser
{
    [JsonPropertyName("ckey")]
    public string CKey { get; set; }
    
    [JsonPropertyName("userIdentifier")]
    public string UserIdentifier { get; set; }
}

public class TgServer
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("identifier")]
    public string Identifier { get; set; }
    
    [JsonPropertyName("port")]
    public int Port { get; set; }
    
    [JsonPropertyName("address")]
    public string Address { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; }
    
    [JsonPropertyName("publicLogs")]
    public string PublicLogsUrl { get; set; }
    
    [JsonPropertyName("rawLogs")]
    public string RawLogsUrl { get; set; }
    
    [JsonPropertyName("round")]
    public int? Round { get; set; }
}

public class TgRawBan : IRawBan
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("admin")]
    public TgUser Admin { get; set; }
    
    [JsonPropertyName("target")]
    public TgUser Target { get; set; }
    
    [JsonPropertyName("unbanner")]
    public TgUser? Unbanner { get; set; }
    
    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; }
    
    [JsonPropertyName("bantime")]
    public DateTimeOffset BanTime { get; set; }
    
    [JsonPropertyName("unbannedTime")]
    public DateTimeOffset? UnbannedTime { get; set; }

    [JsonPropertyName("round")]
    public int Round { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; }
    
    [JsonPropertyName("reason")]
    public string Reason { get; set; }
    
    [JsonPropertyName("server")]
    public TgServer Server { get; set; }
    
    [JsonPropertyName("expiration")]
    public DateTimeOffset? Expiration { get; set; }
    
    [JsonPropertyName("banIds")]
    public List<int> BanIds { get; set; }

    public BanType GetBanType() => Roles.Count == 1 && Roles[0] == "Server" ? BanType.Server : BanType.Job;

    public Ban AsBan(BanSource source)
    {
        var toReturn = new Ban
        {
            SourceNavigation = source,
            BanType = GetBanType(),
            CKey = Target.CKey,
            BannedOn = BanTime.UtcDateTime,
            BannedBy = Admin.CKey,
            Reason = Reason,
            Expires = Expiration?.UtcDateTime,
            UnbannedBy = Unbanner?.CKey,
            BanID = string.Join(";", BanIds)
        };

        // Add job bans if relevant
        if (toReturn.BanType == BanType.Job)
            toReturn.AddJobRange(Roles);

        return toReturn;
    }
}