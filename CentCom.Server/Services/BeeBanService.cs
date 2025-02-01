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

public class BeeBanService(HttpClient client, ILogger<BeeBanService> logger) : HttpBanService(client, logger)
{
    private const int ParallelRequests = 1;
    private static readonly BanSource LrpSource = new() { Name = "bee-lrp" };
    private static readonly BanSource MrpSource = new() { Name = "bee-mrp" };

    protected override string BaseUrl => "https://api.beestation13.com/";

    internal async Task<List<Ban>> GetBansAsync(int page = 1)
    {
        var toReturn = new List<Ban>();
        var content =
            await GetAsync<JsonElement>("bans", new Dictionary<string, string>() { { "page", page.ToString() } });
        foreach (var b in content.GetProperty("data").EnumerateArray())
        {
            var expiryString = b.GetProperty("unbanned_datetime").GetString() ??
                               b.GetProperty("expiration_time").GetString();
            var toAdd = new Ban
            {
                BannedOn = DateTime.Parse(b.GetProperty("bantime").GetString()),
                BannedBy = b.GetProperty("a_ckey").GetString(),
                UnbannedBy = b.GetProperty("unbanned_ckey").GetString(),
                BanType = b.GetProperty("roles").EnumerateArray().Select(x => x.GetString()).Contains("Server")
                    ? BanType.Server
                    : BanType.Job,
                Expires = expiryString == null
                    ? null
                    : DateTime.Parse(expiryString),
                CKey = b.GetProperty("ckey").GetString(),
                Reason = b.GetProperty("reason").GetString(),
                BanID = b.GetProperty("id").GetInt32().ToString(),
                SourceNavigation = ParseBanSource(b.GetProperty("server_name").GetString())
            };

            if (toAdd.BanType == BanType.Job)
            {
                toAdd.AddJobRange(b.GetProperty("roles").EnumerateArray().Select(x => x.GetString()));
            }

            if (b.GetProperty("global_ban").GetInt32() == 1)
            {
                toAdd.AddAttribute(BanAttribute.BeeStationGlobal);
            }

            toReturn.Add(toAdd);
        }

        return toReturn;
    }

    public async Task<List<Ban>> GetBansBatchedAsync(int startpage = 1, int pages = -1)
    {
        var maxPages = await GetNumberOfPagesAsync();
        var range = Enumerable.Range(startpage, pages != -1 ? pages : maxPages + 8); // pad with 8 pages for safety
        var toReturn = new ConcurrentBag<Ban>();

        await range.AsyncParallelForEach(async page =>
        {
            foreach (var b in await GetBansAsync(page))
            {
                toReturn.Add(b);
            }
        }, ParallelRequests);
        return toReturn.ToList();
    }

    internal async Task<int> GetNumberOfPagesAsync() =>
        (await GetAsync<JsonElement>("bans")).GetProperty("pages").GetInt32();

    private static BanSource ParseBanSource(string raw)
    {
        return (raw.ToLower()) switch
        {
            "bs_golden" => LrpSource,
            "bs_sage" => MrpSource,
            "bs_acacia" => MrpSource,
            "bs_linden" => MrpSource,
            _ => throw new Exception(
                $"Failed to convert raw value of Beestation ban source to BanSource: \"{raw}\""),
        };
    }
}