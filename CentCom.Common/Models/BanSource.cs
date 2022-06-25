using System.Collections.Generic;

namespace CentCom.Common.Models;

public sealed class BanSource
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Display { get; set; }
    public RoleplayLevel RoleplayLevel { get; set; }
    public List<Ban> Bans { get; set; }
}