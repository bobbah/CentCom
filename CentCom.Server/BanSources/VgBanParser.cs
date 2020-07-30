using CentCom.Common.Models;
using CentCom.Server.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using CentCom.Common.Data;

namespace CentCom.Server.BanSources
{
    public class VgBanParser : BanParser
    {
        public override Dictionary<string, BanSource> Sources => new Dictionary<string, BanSource>()
        {
            { "vgstation", new BanSource()
            {
                Display = "/vg/station",
                Name = "vgstation",
                RoleplayLevel = RoleplayLevel.Low
            } }
        };
        public override bool SourceSupportsBanIDs => false;
        private VgBanService _banService;

        public VgBanParser(DatabaseContext dbContext, VgBanService banService, ILogger<VgBanParser> logger) : base(dbContext, logger)
        {
            _banService = banService;
        }

        public override async Task<IEnumerable<Ban>> FetchAllBansAsync()
        {
            _logger.LogInformation("Fetching all bans for /vg/station...");
            return await _banService.GetBansAsync();
        }

        public override async Task<IEnumerable<Ban>> FetchNewBansAsync()
        {
            // Note that the /vg/station website only has a single page for bans, so we always do a full refresh
            _logger.LogInformation("Fetching new bans for /vg/station...");
            return await _banService.GetBansAsync();
        }
    }
}
