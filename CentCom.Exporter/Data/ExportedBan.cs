using System;
using CentCom.Common.Models;
using CentCom.Common.Models.Byond;
using CentCom.Common.Models.Rest;

namespace CentCom.Exporter.Data
{
    public class ExportedBan
    {
        public int Id { get; set; }
        public DateTimeOffset BannedAt { get; set; }
        public string Role { get; set; }

        public BanType BanType => Role.Equals("server", StringComparison.InvariantCultureIgnoreCase)
            ? BanType.Server
            : BanType.Job;

        public DateTimeOffset? Expiration { get; set; }
        public string Reason { get; set; }
        public CKey CKey { get; set; }
        public CKey BannedBy { get; set; }
        public DateTimeOffset? Unbanned { get; set; }
        public CKey UnbannedBy { get; set; }

        public static implicit operator RestBan(ExportedBan ban) => new RestBan
        (
            ban.Id,
            ban.BanType,
            ban.CKey,
            ban.BannedAt,
            ban.BannedBy,
            ban.Reason,
            ban.Expiration,
            ban.UnbannedBy,
            ban.BanType == BanType.Job ? new []{ new RestJobBan(ban.Role) } : null
        );
    }
}