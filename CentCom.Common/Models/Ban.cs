using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace CentCom.Common.Models
{
    public class Ban
    {
        private static Regex _keyReplacePattern = new Regex(@"[^a-z0-9]");

        public int Id { get; set; }
        public int Source { get; set; }
        public virtual BanSource SourceNavigation { get; set; }
        public BanType BanType { get; set; }
        public string CKey { get; set; }
        public DateTime BannedOn { get; set; }
        public string BannedBy { get; set; }
        public string Reason { get; set; }
        public DateTime? Expires { get; set; }
        public string UnbannedBy { get; set; }
        public IPAddress IP { get; set; }
        public long? CID { get; set; }
        public string BanID { get; set; }
        public List<JobBan> JobBans { get; set; }

        public void MakeKeyCanonical()
        {
            CKey = CKey == null ? null : _keyReplacePattern.Replace(CKey.ToLower(), "");
        }
    }
}
