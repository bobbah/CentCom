using System.Collections.Generic;
using CentCom.Common.Models;

namespace CentCom.Server.FlatData;

public class FlatDataFile
{
    public uint Version { get; set; }
    public string Name { get; set; }
    public List<BanSource> Sources { get; set; }
    public List<Ban> JobBans { get; set; }
    public List<Ban> ServerBans { get; set; }
}