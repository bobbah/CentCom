using CentCom.Common.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CentCom.Server.FlatData
{
    public class OracleDataSource : IFlatDataSource
    {
        public string SourceDisplayName() => "OracleStation";

        public IEnumerable<Ban> GetBans()
        {
            var source = GetSources().First();
            var toReturn = new List<Ban>();
            var jsonSettings = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
            toReturn.AddRange(JsonSerializer.Deserialize<IEnumerable<Ban>>(File.ReadAllText("FlatData/JSON/Oracle/oracle_bans_job.json"), jsonSettings));
            toReturn.AddRange(JsonSerializer.Deserialize<IEnumerable<Ban>>(File.ReadAllText("FlatData/JSON/Oracle/oracle_bans_server.json"), jsonSettings));
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
                Name = "oracle",
                Display = "OracleStation",
                RoleplayLevel = RoleplayLevel.Medium
            } };
        }
    }
}
