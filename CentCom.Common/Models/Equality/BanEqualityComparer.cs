using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentCom.Common.Models.Equality
{
    public class BanEqualityComparer : IEqualityComparer<Ban>
    {
        public static readonly BanEqualityComparer Instance = new BanEqualityComparer();

        public bool Equals(Ban x, Ban y)
        {
            if (x is null || y is null) return x == y;
            else if (x.BanID != null || y.BanID != null)
            {
                return x.Source == y.Source
                    && x.BanID == y.BanID;
            }
            else
            {
                return x.Source == y.Source
                    && x.BannedOn == y.BannedOn
                    && x.BanType == y.BanType
                    && x.CKey == y.CKey
                    && x.BannedBy == y.BannedBy
                    && x.Reason == y.Reason
                    && x.Expires == y.Expires
                    && x.UnbannedBy == y.UnbannedBy
                    && (x.IP == null || x.IP.Equals(y.IP))
                    && x.CID == y.CID
                    && (x.BanType == BanType.Server
                            || (x.JobBans != null && y.JobBans != null && x.JobBans.SetEquals(y.JobBans)));
            }
        }

        public int GetHashCode(Ban obj)
        {
            var hash = new HashCode();
            if (obj.BanID != null)
            {
                hash.Add(obj.BanID);
                hash.Add(obj.Source);
            }
            else
            {
                hash.Add(obj.Source);
                hash.Add(obj.BannedOn);
                hash.Add(obj.BanType);
                hash.Add(obj.CKey);
                hash.Add(obj.BannedBy);
                hash.Add(obj.Reason);
                hash.Add(obj.Expires);
                hash.Add(obj.UnbannedBy);
                hash.Add(obj.IP);
                hash.Add(obj.CID);
                if (obj.JobBans != null)
                {
                    foreach (var job in obj.JobBans.OrderBy(x => x.Job))
                    {
                        hash.Add(job.Job);
                    }
                }
            }
            return hash.ToHashCode();
        }
    }
}
