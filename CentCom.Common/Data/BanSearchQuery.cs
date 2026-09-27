using System.Linq;
using CentCom.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace CentCom.Common.Data;

public static class BanSearchQuery
{
    public static IQueryable<Ban> ForCKeySubstring(DatabaseContext context, string canonicalKey)
    {
        var query = context.Bans.AsNoTracking();
        if (!context.Database.IsRelational())
            return query.Where(ban => ban.CKey.ToLower().Contains(canonicalKey));

        if (context is MySqlDbContext or MariaDbContext && canonicalKey.Length >= 3)
        {
            var gram = canonicalKey[..3];
            query = query.Where(ban => context.BanCKeyGrams
                .Any(posting => posting.BanId == ban.Id && posting.Gram == gram));
        }

        return query.Where(ban => EF.Functions.Like(ban.CKey.ToLower(), $"%{canonicalKey}%"));
    }
}
