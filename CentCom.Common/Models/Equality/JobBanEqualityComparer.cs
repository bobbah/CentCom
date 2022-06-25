using System;
using System.Collections.Generic;

namespace CentCom.Common.Models.Equality;

public class JobBanEqualityComparer : IEqualityComparer<JobBan>
{
    public static readonly JobBanEqualityComparer Instance = new JobBanEqualityComparer();

    public bool Equals(JobBan x, JobBan y)
    {
        return (x is null || y is null) ? x == y : x.Job == y.Job;
    }

    public int GetHashCode(JobBan obj)
    {
        return HashCode.Combine(obj.Job);
    }
}