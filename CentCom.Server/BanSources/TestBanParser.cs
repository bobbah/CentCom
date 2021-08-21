using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentCom.Common.Data;
using CentCom.Common.Models;
using CentCom.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CentCom.Server.BanSources
{
    public class TestBanParser : BanParser
    {
        private readonly ExporterService _banService;

        public TestBanParser(DatabaseContext dbContext, ILogger<TestBanParser> logger, ExporterService banService) :
            base(dbContext, logger)
        {
            _banService = banService;
        }

        protected override Dictionary<string, BanSource> Sources => new Dictionary<string, BanSource>()
        {
            {
                "teststation", new BanSource()
                {
                    Display = "Test Station",
                    Name = "teststation",
                    RoleplayLevel = RoleplayLevel.Low
                }
            }
        };

        protected override bool SourceSupportsBanIDs => true;
        protected override string Name => "Test";

        public override async Task<IEnumerable<Ban>> FetchNewBansAsync()
        {
            Logger.LogInformation("Fetching new bans for teststation...");
            var recent = await DbContext.Bans
                .Where(x => Sources.Keys.Contains(x.SourceNavigation.Name))
                .OrderByDescending(x => x.BannedOn)
                .Take(5)
                .Include(x => x.JobBans)
                .Include(x => x.SourceNavigation)
                .ToListAsync();
            return await _banService.GetBansBatchedAsync(searchFor: recent.Select(x => int.Parse(x.BanID)));
        }

        public override async Task<IEnumerable<Ban>> FetchAllBansAsync()
        {
            Logger.LogInformation("Fetching all bans for teststation...");
            return await _banService.GetBansBatchedAsync();
        }
    }
}