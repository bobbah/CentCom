using System.Collections.Generic;
using System.Linq;
using CentCom.Common.Abstract;
using CentCom.Common.Models;
using CentCom.Common.Models.Rest;

namespace CentCom.Exporter.Data.Clustering;

public class ParadiseBanClusterer : IBanClusterer {
    /// <inheritdoc/>
    /// <remarks>
    /// Job bans are considered to be in a group if the ckey, banning ckey, reason,
    /// expiration timestamp, and unbanned ckey are the same.
    /// </remarks>
    public IEnumerable<IRestBan> ClusterBans(IEnumerable<IRestBan> bans) {
        var clusteredBans = bans.Where(x => x.BanType == BanType.Server).ToList();
        clusteredBans.AddRange(bans.Where(x => x.BanType == BanType.Job).GroupBy(x =>
            new { x.CKey, x.BannedBy, x.Reason, x.UnbannedBy, x.RoundId }).Select(group => {
                var ban = group.Last();
                return new RestBan(ban.Id, ban.BanType, ban.CKey, ban.BannedOn, ban.BannedBy, ban.Reason, ban.Expires,
                    ban.UnbannedBy,
                    group.SelectMany(j => j.JobBans)
                        .Select(j => j.Job)
                        .Distinct()
                        .Select(j => (IRestJobBan)new RestJobBan(j)).ToList(),
                    ban.RoundId);
            }));
        return clusteredBans.OrderByDescending(x => x.Id);
    }
}
