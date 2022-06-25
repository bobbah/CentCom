using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CentCom.Common.Models.Equality;

namespace CentCom.Common.Models;

public class Ban
{
    public Ban()
    {
        JobBans = new HashSet<JobBan>(JobBanEqualityComparer.Instance);
    }

    public int Id { get; set; }
    public int Source { get; set; }

    [JsonIgnore]
    public virtual BanSource SourceNavigation { get; set; }

    public BanType BanType { get; set; }
    public string CKey { get; set; }
    public DateTime BannedOn { get; set; }
    public string BannedBy { get; set; }
    public string Reason { get; set; }
    public DateTime? Expires { get; set; }
    public string UnbannedBy { get; set; }
    public string BanID { get; set; }
    public HashSet<JobBan> JobBans { get; set; }
    public BanAttribute BanAttributes { get; set; }
}