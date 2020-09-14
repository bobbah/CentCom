using CentCom.Common.Models;
using CentCom.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentCom.Common.Data;

namespace CentCom.Server.BanSources
{
    public class BeeBanParser : BanParser
    {
        public override Dictionary<string, BanSource> Sources => new Dictionary<string, BanSource>()
        {
            { "bee-lrp", new BanSource()
            {
                Display = "Beestation LRP",
                Name = "bee-lrp",
                RoleplayLevel = RoleplayLevel.Low
            } },
            { "bee-mrp", new BanSource()
            {
                Display = "Beestation MRP",
                Name = "bee-mrp",
                RoleplayLevel = RoleplayLevel.Medium
            } }
        };
        public override bool SourceSupportsBanIDs => true;
        private BeeBanService _banService;
        private const int PAGES_PER_BATCH = 12;

        public BeeBanParser(DatabaseContext dbContext, BeeBanService banService, ILogger<BeeBanParser> logger) : base(dbContext, logger)
        {
            _banService = banService;
        }

        public override async Task<IEnumerable<Ban>> FetchNewBansAsync()
        {
            _logger.LogInformation("Getting new bans for Beestation...");
            var recent = await _dbContext.Bans
                .Where(x => Sources.Keys.Contains(x.SourceNavigation.Name))
                .OrderByDescending(x => x.BannedOn)
                .Take(5)
                .Include(x => x.JobBans)
                .Include(x => x.SourceNavigation)
                .ToListAsync();
            var foundBans = new List<Ban>();
            var page = 1;

            while (true)
            {
                var batch = await _banService.GetBansBatchedAsync(page, PAGES_PER_BATCH);
                foundBans.AddRange(batch);
                if (batch.Count() == 0 || batch.Any(x => recent.Any(y => y.BanID == x.BanID)))
                {
                    break;
                }
                page += PAGES_PER_BATCH;
            }

            return foundBans;
        }

        public override async Task<IEnumerable<Ban>> FetchAllBansAsync()
        {
            _logger.LogInformation("Getting all bans for Beestation...");
            return await _banService.GetBansBatchedAsync();
        }
    }
}
