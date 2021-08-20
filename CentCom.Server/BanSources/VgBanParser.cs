using CentCom.Common.Data;
using CentCom.Common.Models;
using CentCom.Server.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CentCom.Server.BanSources
{
    public class VgBanParser : BanParser
    {
        protected override Dictionary<string, BanSource> Sources => new Dictionary<string, BanSource>()
        {
            { "vgstation", new BanSource()
            {
                Display = "/vg/station",
                Name = "vgstation",
                RoleplayLevel = RoleplayLevel.Low
            } }
        };

        protected override bool SourceSupportsBanIDs => false;
        protected override string Name => "/vg/station";
        private readonly VgBanService _banService;

        public VgBanParser(DatabaseContext dbContext, VgBanService banService, ILogger<VgBanParser> logger) : base(dbContext, logger)
        {
            _banService = banService;
        }

        public override async Task<IEnumerable<Ban>> FetchAllBansAsync()
        {
            Logger.LogInformation("Fetching all bans for /vg/station...");
            return await _banService.GetBansAsync();
        }

        public override async Task<IEnumerable<Ban>> FetchNewBansAsync()
        {
            // Note that the /vg/station website only has a single page for bans, so we always do a full refresh
            Logger.LogInformation("Fetching new bans for /vg/station...");
            return await _banService.GetBansAsync();
        }
    }
}
