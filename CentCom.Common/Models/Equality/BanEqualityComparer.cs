using System;
using System.Collections.Generic;
using System.Linq;

namespace CentCom.Common.Models.Equality;

public class BanEqualityComparer : IEqualityComparer<Ban>
{
    public static readonly BanEqualityComparer Instance = new BanEqualityComparer();

    public bool Equals(Ban x, Ban y)
    {
        if (x is null || y is null) return x == y;
        if (x.BanID != null || y.BanID != null)
        {
            return x.Source == y.Source
                   && x.BanID == y.BanID
                   && x.BanAttributes == y.BanAttributes;
        }

        return x.Source == y.Source
               && x.BannedOn == y.BannedOn
               && x.BanType == y.BanType
               && x.CKey == y.CKey
               && x.BannedBy == y.BannedBy
               && x.Reason == y.Reason
               && x.Expires == y.Expires
               && x.UnbannedBy == y.UnbannedBy
               && (x.BanType == BanType.Server || ((x.JobBans == y.JobBans && x.JobBans == null) || x.JobBans.SetEquals(y.JobBans)))
               && x.BanAttributes == y.BanAttributes;
    }

    public int GetHashCode(Ban obj)
    {
        // Add the hashable components that all bans have
        var hash = new HashCode();
        hash.Add(obj.Source);
        hash.Add(obj.BanAttributes);

        // Add those specific to bans with IDs or those without
        if (obj.BanID != null)
        {
            hash.Add(obj.BanID);
        }
        else
        {
            hash.Add(obj.BannedOn);
            hash.Add(obj.BanType);
            hash.Add(obj.CKey);
            hash.Add(obj.BannedBy);
            hash.Add(obj.Reason);
            hash.Add(obj.Expires);
            hash.Add(obj.UnbannedBy);
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