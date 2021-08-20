using System.Collections.Generic;
using CentCom.Common.Models;

namespace CentCom.Server.FlatData
{
    public class FlatDataFile
    {
        public uint Version { get; set; }
        public string Name { get; set; }
        public IEnumerable<BanSource> Sources { get; set; }
        public IEnumerable<Ban> JobBans { get; set; }
        public IEnumerable<Ban> ServerBans { get; set; }
    }
}
