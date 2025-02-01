using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CentCom.Common.Extensions;
using CentCom.Common.Models;
using CentCom.Server.Extensions;
using Microsoft.Extensions.Logging;

namespace CentCom.Server.Services;

/// <summary>
/// TGMC Ban Service for getting bans from the API
/// </summary>
/// <remarks>
/// Note that the data is provided in a flat tg format, meaning
/// that each role in a jobban has its own ban ID. Thus, our strategy
/// for using the paging must account for the possibility of a job ban
/// spanning two seperate pages.
/// </remarks>
public class TGMCBanService(HttpClient client, ILogger<TGMCBanService> logger) : HttpBanService(client, logger)
{
    private const int RecordsPerPage = 100;
    private static readonly BanSource BanSource = new() { Name = "tgmc" };

    protected override string BaseUrl => "https://statbus.psykzz.com/api/";

    public async Task<List<Ban>> GetBansAsync(int page = 1)
    {
        var toReturn = new List<Ban>();
        var dirtyBans = new List<Ban>();
        var content = await GetAsync<Dictionary<string, JsonElement>>($"bans/{page}",
            new Dictionary<string, string>() { { "limit", RecordsPerPage.ToString() } });
        foreach (var bh in content["bans"].EnumerateObject())
        {
            var ban = bh.Value;

            // Ban expiration could be based on the expiration time field or the existance of the unbanned datetime
            // field, so we have to check both.
            var expiration = ban.GetProperty("unbanned_datetime").GetString() == null
                ? (DateTime?)null
                : DateTime.Parse(ban.GetProperty("unbanned_datetime").GetString());
            if (!expiration.HasValue)
            {
                expiration = ban.GetProperty("expiration_time").GetString() == null
                    ? null
                    : DateTime.Parse(ban.GetProperty("expiration_time").GetString());
            }

            // Get ban
            var toAdd = new Ban
            {
                BanID = bh.Name,
                BannedBy = ban.GetProperty("admin").GetString(),
                BannedOn = DateTime.Parse(ban.GetProperty("bantime").ToString()),
                CKey = ban.GetProperty("ckey").GetString(),
                Expires = expiration,
                Reason = ban.GetProperty("reason").ToString(),
                BanType = ban.GetProperty("role").GetString().ToLower() == "server" ? BanType.Server : BanType.Job,
                SourceNavigation = BanSource
            };

            // Add jobban if relevant
            if (toAdd.BanType == BanType.Job)
            {
                toAdd.AddJob(ban.GetProperty("role").GetString());
                dirtyBans.Add(toAdd);
            }
            else
            {
                toReturn.Add(toAdd);
            }
        }

        // Group jobbans
        foreach (var group in dirtyBans.GroupBy(x => new { x.CKey, x.BannedOn }))
        {
            var firstBan = group.OrderBy(x => x.BanID).First();
            firstBan.AddJobRange(group.SelectMany(x => x.JobBans).Select(x => x.Job));
            toReturn.Add(firstBan);
        }

        return toReturn;
    }

    public async Task<List<Ban>> GetBansBatchedAsync(int startPage = 1, int pages = -1)
    {
        var maxPages = await GetNumberOfPagesAsync();
        var range = Enumerable.Range(startPage, pages != -1 ? Math.Min(startPage + pages, maxPages) : maxPages);
        var dirtyBans = new ConcurrentBag<Ban>();
        await range.AsyncParallelForEach(async page =>
        {
            foreach (var b in await GetBansAsync(page))
            {
                dirtyBans.Add(b);
            }
        }, 12);


        if (dirtyBans.IsEmpty)
            return [];

        // We have to ensure that our jobs are correctly grouped due to possible errors with paging
        var cleanBans = new List<Ban>(dirtyBans.Where(x => x.BanType == BanType.Server));
        foreach (var group in dirtyBans.Where(x => x.BanType == BanType.Job).GroupBy(x => new { x.CKey, x.BannedOn }))
        {
            var firstBan = group.OrderBy(x => x.BanID).First();
            firstBan.AddJobRange(group.SelectMany(x => x.JobBans).Select(x => x.Job));
            cleanBans.Add(firstBan);
        }

        // Check for the possibility of a job ban spanning multiple pages
        cleanBans = cleanBans.OrderBy(x => int.Parse(x.BanID)).ToList();
        if (startPage != 1 && cleanBans.First().BanType == BanType.Job)
        {
            // Discard the first ban if it is a job ban, as it may be incomplete.
            // The alternate would be walking backwards in the page list, but that
            // is not an optimal solution
            cleanBans.RemoveAt(0);
        }

        if (pages != -1 && startPage + pages < maxPages && cleanBans.LastOrDefault()?.BanType == BanType.Job)
        {
            // Discard the last ban if it is a job ban as it may be incomplete.
            // Same reasoning above, except it would require a pagewalk forward.
            cleanBans.RemoveAt(cleanBans.Count - 1);
        }

        return cleanBans;
    }

    public async Task<int> GetNumberOfPagesAsync()
    {
        var content = await GetAsync<Dictionary<string, JsonElement>>("bans/1",
            new Dictionary<string, string>() { { "limit", RecordsPerPage.ToString() } });
        if (content["page"].TryGetProperty("total", out var lastpage))
        {
            return lastpage.GetInt32();
        }

        throw new Exception("Failed to find the last page number in the response from TGMC's API");
    }
}