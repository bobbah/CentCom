using System;

namespace CentCom.Common.Models;

public class KeySummary
{
    public string CKey { get; set; }
    public int JobBans { get; set; }
    public int ServerBans { get; set; }
    public DateTime LatestBan { get; set; }
}