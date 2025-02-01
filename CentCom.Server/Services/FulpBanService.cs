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

public class FulpBanService(HttpClient client, ILogger<FulpBanService> logger) : HttpBanService(client, logger)
{
    private const int RecordsPerPage = 50;
    private static readonly BanSource BanSource = new() { Name = "fulp" };

    protected override string BaseUrl => "https://api.fulp.gg/";

    public async Task<List<Ban>> GetBansAsync(int page = 1)
    {
        var content = await GetAsync<Dictionary<string, JsonElement>>($"bans/{RecordsPerPage}/{page}");
        var toReturn = new List<Ban>();
        foreach (var ban in content["value"].GetProperty("bans").EnumerateArray())
        {
            // Need to get both the expiration as well as the unbanned time as they can differ
            DateTime? expiration = null;
            var unbannedTime = ban.GetProperty("unbannedTime").GetString();
            var expireTime = ban.GetProperty("banExpireTime").GetString();
            if (unbannedTime != null)
                expiration = DateTime.Parse(unbannedTime);
            if (expireTime != null)
                expiration ??= DateTime.Parse(expireTime);

            // Get ban
            var toAdd = new Ban
            {
                BannedOn = DateTime.Parse(ban.GetProperty("banApplyTime").GetString()),
                BannedBy = ban.GetProperty("adminCkey").GetString(),
                BanType = ban.GetProperty("role")[0].GetString().ToLower() == "server"
                    ? BanType.Server
                    : BanType.Job,
                Expires = expiration,
                CKey = ban.GetProperty("bannedCkey").GetString(),
                Reason = ban.GetProperty("reason").GetString(),
                SourceNavigation = BanSource
            };

            // Add jobs if relevant
            if (toAdd.BanType == BanType.Job)
            {
                toAdd.AddJobRange(ban.GetProperty("role").EnumerateArray().Select(x => x.GetString()));
            }

            toReturn.Add(toAdd);
        }

        return toReturn;
    }

    public async Task<List<Ban>> GetBansBatchedAsync(int startPage = 1, int pages = -1)
    {
        var maxPages = await GetNumberOfPagesAsync();
        var range = Enumerable.Range(startPage, pages != -1 ? Math.Min(startPage + pages, maxPages) : maxPages);
        var toReturn = new ConcurrentBag<Ban>();
        await range.AsyncParallelForEach(async page =>
        {
            foreach (var b in await GetBansAsync(page))
            {
                toReturn.Add(b);
            }
        }, 6);
        return toReturn.ToList();
    }

    public async Task<int> GetNumberOfPagesAsync()
    {
        var content = await GetAsync<Dictionary<string, JsonElement>>($"bans/{RecordsPerPage}/1");
        if (content["value"].TryGetProperty("lastPage", out var lastpage))
        {
            return lastpage.GetInt32();
        }

        throw new Exception("Failed to find the last page number in the response from Fulpstation's API");
    }
}