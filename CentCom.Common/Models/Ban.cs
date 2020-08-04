using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CentCom.Common.Models
{
    public class Ban :IEquatable<Ban>
    {
        private static Regex _keyReplacePattern = new Regex(@"[^a-z0-9]");

        public int Id { get; set; }
        public int Source { get; set; }
        [JsonIgnore]
        public virtual BanSource SourceNavigation { get; set; }
        public BanType BanType { get; set; }
        public string CKey { get; set; }
        public DateTime BannedOn { get; set; }
        public string BannedBy { get; set; }
        public string Reason { get; set; }
        public DateTime? Expires { get; set; }
        public string UnbannedBy { get; set; }
        [JsonConverter(typeof(JsonIPAddressConverter))]
        public IPAddress IP { get; set; }
        public long? CID { get; set; }
        public string BanID { get; set; }
        public HashSet<JobBan> JobBans { get; set; }

        /// <summary>
        /// Determines if two bans are equal, using their values
        /// </summary>
        /// <param name="other">The ban to compare against</param>
        /// <returns>If the two bans are equal by their values</returns>
        /// <remarks>
        /// Note that these equals are really just used for determining if
        /// the contents of two bans are the same, at which point they're equal.
        /// </remarks>
        public bool Equals(Ban other)
        {
            if (other == null) return false;
            else if (BanID != null || other.BanID != null)
            {
                return Source == other.Source
                    && BanID == other.BanID;
            }
            else
            {
                return Source == other.Source
                    && BannedOn == other.BannedOn
                    && BanType == other.BanType
                    && CKey == other.CKey
                    && BannedBy == other.BannedBy
                    && Reason == other.Reason
                    && Expires == other.Expires
                    && UnbannedBy == other.UnbannedBy
                    && (IP == null || IP.Equals(other.IP))
                    && CID == other.CID
                    && (BanType == BanType.Server
                            || (JobBans != null && other.JobBans != null && JobBans.SetEquals(other.JobBans)));
            }
        }

        public static bool operator ==(Ban a, Ban b)
        {
            return (a is null && b is null) || a.Equals(b);
        }

        public static bool operator !=(Ban a, Ban b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return (obj is Ban ban) && Equals(ban);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            if (BanID != null)
            {
                hash.Add(BanID);
                hash.Add(Source);
            }
            else
            {
                hash.Add(Source);
                hash.Add(BannedOn);
                hash.Add(BanType);
                hash.Add(CKey);
                hash.Add(BannedBy);
                hash.Add(Reason);
                hash.Add(Expires);
                hash.Add(UnbannedBy);
                hash.Add(IP);
                hash.Add(CID);
                if (JobBans != null)
                {
                    foreach (var job in JobBans.OrderBy(x => x.Job))
                    {
                        hash.Add(job.Job, StringComparer.OrdinalIgnoreCase);
                    }
                }
            }
            return hash.ToHashCode();
        }

        public void MakeKeysCanonical()
        {
            CKey = CKey == null ? null : GetCanonicalKey(CKey);
            BannedBy = BannedBy == null ? null : GetCanonicalKey(BannedBy);
            UnbannedBy = UnbannedBy == null ? null : GetCanonicalKey(UnbannedBy);
        }

        public static string GetCanonicalKey(string raw)
        {
            if (raw == null)
            {
                throw new ArgumentNullException(nameof(raw));
            }

            return _keyReplacePattern.Replace(raw.ToLower(), "");
        }
    }
}
