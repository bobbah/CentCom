using CentCom.Common.Data;
using CentCom.Common.Models;
using CentCom.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CentCom.Server.BanSources
{
    public class YogBanParser : BanParser
    {
        public override Dictionary<string, BanSource> Sources => new Dictionary<string, BanSource>()
        {
            { "yogstation", new BanSource()
            {
                Display = "YogStation",
                Name = "yogstation",
                RoleplayLevel = RoleplayLevel.Medium
            } }
        };
        public override bool SourceSupportsBanIDs => true;
        private readonly YogBanService _banService;
        private const int PagesPerBatch = 12;

        public YogBanParser(DatabaseContext dbContext, YogBanService banService, ILogger<YogBanParser> logger) : base(dbContext, logger)
        {
            _banService = banService;
        }

        public override async Task<IEnumerable<Ban>> FetchNewBansAsync()
        {
            _logger.LogInformation("Getting new bans for YogStation...");
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
                var batch = (await _banService.GetBansBatchedAsync(page, PagesPerBatch)).ToArray();
                foundBans.AddRange(batch);
                if (!batch.Any() || batch.Any(x => recent.Any(y => y.BanID == x.BanID)))
                {
                    break;
                }
                page += PagesPerBatch;
            }

            return foundBans;
        }

        public override async Task<IEnumerable<Ban>> FetchAllBansAsync()
        {
            _logger.LogInformation("Getting all bans for YogStation...");
            return await _banService.GetBansBatchedAsync();
        }
    }
}
