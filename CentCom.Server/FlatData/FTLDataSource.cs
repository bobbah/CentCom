using CentCom.Common.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CentCom.Server.FlatData
{
    public class FTLDataSource : IFlatDataSource
    {
        public string SourceDisplayName() => "FTL13";

        public IEnumerable<Ban> GetBans()
        {
            var source = GetSources().First();
            var toReturn = new List<Ban>();
            var jsonSettings = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
            toReturn.AddRange(JsonSerializer.Deserialize<IEnumerable<Ban>>(File.ReadAllText("FlatData/JSON/FTL/ftl_bans_job.json"), jsonSettings));
            toReturn.AddRange(JsonSerializer.Deserialize<IEnumerable<Ban>>(File.ReadAllText("FlatData/JSON/FTL/ftl_bans_server.json"), jsonSettings));
            foreach (var b in toReturn)
            {
                b.SourceNavigation = source;
            }
            return toReturn;
        }

        public IEnumerable<BanSource> GetSources()
        {
            return new List<BanSource>() { new BanSource()
            {
                Name = "ftl13",
                Display = "FTL13",
                RoleplayLevel = RoleplayLevel.Medium
            } };
        }
    }
}
