using System.Text.Json.Serialization;

namespace CentCom.Common.Models;

public class JobBan
{
    public int BanId { get; set; }

    [JsonIgnore]
    public virtual Ban BanNavigation { get; set; }

    public string Job { get; set; }
}