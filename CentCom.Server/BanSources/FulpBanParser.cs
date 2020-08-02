using CentCom.Common.Data;
using CentCom.Common.Models;
using CentCom.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentCom.Server.BanSources
{
    public class FulpBanParser : BanParser
    {
        public override Dictionary<string, BanSource> Sources => new Dictionary<string, BanSource>()
        {
            { "fulp", new BanSource()
            {
                Display = "FulpStation",
                Name = "fulp",
                RoleplayLevel = RoleplayLevel.Low
            } }
        };
        private FulpBanService _banService;
        private const int PAGES_PER_BATCH = 12;

        public FulpBanParser(DatabaseContext dbContext, FulpBanService banService, ILogger<FulpBanParser> logger) : base(dbContext, logger)
        {
            _banService = banService;
            _logger = logger;
        }

        public override async Task<IEnumerable<Ban>> FetchAllBansAsync()
        {
            _logger.LogInformation("Getting all bans for FulpStation...");
            return await _banService.GetBansBatchedAsync();
        }

        public override async Task<IEnumerable<Ban>> FetchNewBansAsync()
        {
            _logger.LogInformation("Getting new bans for FulpStation...");
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
                if (batch.Count() == 0 || batch.Any(x => recent.Any(y => y.BannedOn == x.BannedOn && y.CKey == y.CKey)))
                {
                    break;
                }
            }

            return foundBans;
        }
    }
}
