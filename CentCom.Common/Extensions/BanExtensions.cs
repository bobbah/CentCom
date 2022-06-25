using System.Collections.Generic;
using CentCom.Common.Models;

namespace CentCom.Common.Extensions;

public static class BanExtensions
{
    public static void MakeKeysCanonical(this Ban ban)
    {
        ban.CKey = ban.CKey == null ? null : KeyUtilities.GetCanonicalKey(ban.CKey);
        ban.BannedBy = ban.BannedBy == null ? null : KeyUtilities.GetCanonicalKey(ban.BannedBy);
        ban.UnbannedBy = ban.UnbannedBy == null ? null : KeyUtilities.GetCanonicalKey(ban.UnbannedBy);
    }

    public static string CleanJob(string job)
    {
        return job.ToLower();
    }

    public static bool AddJob(this Ban ban, string job)
    {
        return ban.JobBans.Add(new JobBan { Job = CleanJob(job), BanId = ban.Id, BanNavigation = ban });
    }

    public static void AddJobRange(this Ban ban, IEnumerable<string> jobs)
    {
        foreach (var job in jobs)
        {
            ban.AddJob(job);
        }
    }

    public static void AddAttribute(this Ban ban, BanAttribute attribute)
    {
        ban.BanAttributes |= attribute;
    }

    public static void AddAttributeRange(this Ban ban, IEnumerable<BanAttribute> attributes)
    {
        foreach (var attribute in attributes)
        {
            ban.BanAttributes |= attribute;
        }
    }

    public static void RemoveAttribute(this Ban ban, BanAttribute attribute)
    {
        ban.BanAttributes &= ~attribute;
    }
}