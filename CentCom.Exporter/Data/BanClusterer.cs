using System.Collections.Generic;
using System.Linq;
using CentCom.Common.Abstract;
using CentCom.Common.Models;
using CentCom.Common.Models.Rest;

namespace CentCom.Exporter.Data
{
    /// <summary>
    /// Provides utilities for clustering bans such that job bans are correctly grouped into one ban
    /// </summary>
    public static class BanClusterer
    {
        /// <summary>
        /// Clusters a collection of IRestBans to ensure that all job bans are grouped appropriately
        /// </summary>
        /// <param name="bans">The bans to cluster</param>
        /// <returns>The collection of bans with all job bans clustered appropriately</returns>
        /// <remarks>
        /// Job bans are considered to be in a group if the ckey, banning ckey, reason,
        /// banned timestamp, expiration timestamp, and unbanned ckey are the same.
        /// </remarks>
        public static IEnumerable<IRestBan> ClusterBans(IEnumerable<IRestBan> bans)
        {
            var clusteredBans = bans.Where(x => x.BanType == BanType.Server).ToList();
            clusteredBans.AddRange(bans.Where(x => x.BanType == BanType.Job).GroupBy(x =>
                new { x.CKey, x.BannedBy, x.Reason, x.BannedOn, x.Expires, x.UnbannedBy }).Select(group =>
            {
                var ban = group.Last();
                return new RestBan(ban.Id, ban.BanType, ban.CKey, ban.BannedOn, ban.BannedBy, ban.Reason, ban.Expires,
                    ban.UnbannedBy,
                    group.SelectMany(j => j.JobBans)
                        .Select(j => j.Job)
                        .Distinct()
                        .Select(j => (IRestJobBan)new RestJobBan(j)).ToList());
            }));
            return clusteredBans.OrderByDescending(x => x.Id);
        }
    }
}