using CentCom.Common.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CentCom.Server.Extensions
{
    public static class BanExtensions
    {
        private static Regex _keyReplacePattern = new Regex(@"[^a-z0-9]");

        public static void MakeKeysCanonical(this Ban ban)
        {
            ban.CKey = ban.CKey == null ? null : GetCanonicalKey(ban.CKey);
            ban.BannedBy = ban.BannedBy == null ? null : GetCanonicalKey(ban.BannedBy);
            ban.UnbannedBy = ban.UnbannedBy == null ? null : GetCanonicalKey(ban.UnbannedBy);
        }

        public static string GetCanonicalKey(string raw)
        {
            if (raw == null)
            {
                throw new ArgumentNullException(nameof(raw));
            }

            return _keyReplacePattern.Replace(raw.ToLower(), "");
        }

        public static string CleanJob(string job)
        {
            return job.ToLower();
        }

        public static bool AddJob(this Ban ban, string job)
        {
            return ban.JobBans.Add(new JobBan() { Job = CleanJob(job), BanId = ban.Id, BanNavigation = ban });
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
}
