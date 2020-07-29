using AngleSharp;
using CentCom.Common.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CentCom.Server.Services
{
    public class VgBanService
    {
        private static BanSource _source = new BanSource() { Name = "vgstation" };
        private ILogger _logger;

        public VgBanService(ILogger<VgBanService> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<Ban>> GetBansAsync()
        {
            var toReturn = new List<Ban>();
            var config = AngleSharp.Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync("https://ss13.moe/index.php/bans");
            var tables = document.QuerySelectorAll("form > table > tbody");
            var banTable = tables[0];
            var jobTable = tables[1];

            for (var i = 1; i < banTable.Children.Length; i++)
            {
                var cursor = banTable.Children[i];
                var ckey = cursor.Children[0].Children[0].TextContent.Trim();
                DateTimeOffset date = DateTime.SpecifyKind(cursor.Children[0].Children[0].GetAttribute("title") == "0000-00-00 00:00:00" ? DateTime.MinValue : DateTime.Parse(cursor.Children[0].Children[0].GetAttribute("title").Trim()), DateTimeKind.Utc);
                IPAddress ip = null;
                long? cid = null;
                if (cursor.Children[0].Children.Length > 1)
                {
                    ip = IPAddress.Parse(cursor.Children[0].Children[1].Children[0].Children[0].Children[1].TextContent.Trim());
                    cid = cursor.Children[0].Children[1].Children[0].Children.Length > 1 ? long.Parse(cursor.Children[0].Children[1].Children[0].Children[1].Children[1].TextContent.Trim()) : (long?)null;
                }
                var reason = cursor.Children[1].TextContent.Trim();
                var bannedBy = cursor.Children[2].TextContent.Trim();
                var expiresText = cursor.Children[3].TextContent.Trim();
                DateTimeOffset? expires = null;
                if (DateTime.TryParse(expiresText, out var d))
                {
                    expires = DateTime.SpecifyKind(d, DateTimeKind.Utc);
                }

                toReturn.Add(new Ban()
                {
                    CKey = ckey,
                    BannedOn = date.UtcDateTime,
                    BannedBy = bannedBy,
                    Reason = reason,
                    Expires = expires.HasValue ? expires.Value.UtcDateTime : (DateTime?)null,
                    IP = ip,
                    CID = cid,
                    BanType = BanType.Server,
                    SourceNavigation = _source
                });
            }

            for (var i = 1; i < jobTable.Children.Length; i++)
            {
                var cursor = jobTable.Children[i];
                var bannedDetails = cursor.Children[0];
                var ckey = bannedDetails.Children[0].TextContent.Trim();
                DateTimeOffset date = DateTime.SpecifyKind(cursor.Children[0].Children[0].GetAttribute("title") == "0000-00-00 00:00:00" ? DateTime.MinValue : DateTime.Parse(cursor.Children[0].Children[0].GetAttribute("title").Trim()), DateTimeKind.Utc);
                IPAddress ip = null;
                long? cid = null;
                if (bannedDetails.Children.Length > 1)
                {
                    ip = IPAddress.Parse(bannedDetails.Children[1].Children[0].Children[0].Children[1].TextContent.Trim());
                    cid = bannedDetails.Children[1].Children[0].Children.Length > 1 ? long.Parse(bannedDetails.Children[1].Children[0].Children[1].Children[1].TextContent.Trim()) : (long?)null;
                }
                var jobDetails = cursor.QuerySelector(".clmJobs");
                var jobs = jobDetails.QuerySelectorAll("a").Select(x => x.TextContent.Trim()).Distinct();
                var reason = cursor.Children[2].TextContent.Trim();
                var bannedBy = cursor.Children[3].TextContent.Trim();
                var expiresText = cursor.Children[4].TextContent.Trim();
                DateTimeOffset? expires = null;
                if (DateTime.TryParse(expiresText, out var d))
                {
                    expires = DateTime.SpecifyKind(d, DateTimeKind.Utc);
                }

                toReturn.Add(new Ban()
                {
                    CKey = ckey,
                    BanType = BanType.Job,
                    JobBans = jobs.Select(x => x.ToLower()).ToHashSet().Select(x => new JobBan() { Job = x }).ToList(),
                    Reason = reason,
                    BannedBy = bannedBy,
                    BannedOn = date.UtcDateTime,
                    Expires = expires.HasValue ? expires.Value.UtcDateTime : (DateTime?)null,
                    IP = ip,
                    CID = cid,
                    SourceNavigation = _source
                });
            }

            return toReturn;
        }
    }
}
