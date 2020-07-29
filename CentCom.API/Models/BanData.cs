using CentCom.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CentCom.API.Models
{
    public class BanData
    {
        public int ID { get; set; }
        public int SourceID { get; set; }
        public string SourceName { get; set; }
        public RoleplayLevel SourceRoleplayLevel { get; set; }
        public BanType Type { get; set; }
        public string CKey { get; set; }
        public DateTime BannedOn { get; set; }
        public string BannedBy { get; set; }
        public string Reason { get; set; }
        public DateTime? Expires { get; set; }
        public string UnbannedBy { get; set; }
        public string IP { get; set; }
        public long? CID { get; set; }
        public string BanID { get; set; }
        public List<string> Jobs { get; set; }

        public static BanData FromBan(Ban ban)
        {
            return new BanData()
            {
                ID = ban.Id,
                SourceID = ban.Source,
                SourceName = ban.SourceNavigation.Display,
                SourceRoleplayLevel = ban.SourceNavigation.RoleplayLevel,
                Type = ban.BanType,
                CKey = ban.CKey,
                BannedOn = ban.BannedOn,
                BannedBy = ban.BannedBy,
                Reason = ban.Reason,
                Expires = ban.Expires,
                UnbannedBy = ban.UnbannedBy,
                IP = ban.IP?.ToString(),
                CID = ban.CID,
                BanID = ban.BanID,
                Jobs = (ban.JobBans?.Count > 0) ? ban.JobBans.Select(x => x.Job).ToList() : null
            };
        }

        public bool ShouldSerializeJobs()
        {
            return Type == BanType.Job;
        }
    }
}
