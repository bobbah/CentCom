using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CentCom.Common.Extensions;
using CentCom.Common.Models;
using Microsoft.Extensions.Logging;

namespace CentCom.Server.Services;

public class YogBanService(HttpClient client, ILogger<YogBanService> logger) : HttpBanService(client, logger)
{
    private const int ParallelRequests = 12;
    private const int RequestsPerMinute = 60;
    private static readonly BanSource BanSource = new() { Name = "yogstation" };
    private readonly Regex _pagesPattern = new(@"<a class=""pagination-link[^>]+>(?<pagenum>[0-9]+)<\/a>", RegexOptions.Compiled | RegexOptions.Multiline);

    protected override string BaseUrl => "https://yogstation.net/";

    private async Task<List<Ban>> GetBansAsync(int page = 1)
    {
        var toReturn = new List<Ban>();
        var content = await GetAsync<List<Dictionary<string, JsonElement>>>("bans",
            new Dictionary<string, string>() { { "json", "1" }, { "page", page.ToString() }, { "amount", "1000" } });
        
        foreach (var b in content)
        {
            var expiryString = b["unbanned_datetime"].GetString() ?? b["expiration_time"].GetString();
            var toAdd = new Ban
            {
                BannedOn = DateTime.Parse(b["bantime"].GetString()),
                BannedBy = b["a_ckey"].GetString(),
                UnbannedBy = b["unbanned_ckey"].GetString(),
                BanType = b["roles"].EnumerateArray().Select(x => x.GetString()).Contains("Server")
                    ? BanType.Server
                    : BanType.Job,
                Expires = expiryString == null ? null : DateTime.Parse(expiryString),
                CKey = b["ckey"].GetString(),
                Reason = b["reason"].GetString(),
                BanID = b["id"].GetInt32().ToString(),
                SourceNavigation = BanSource
            };

            if (toAdd.BanType == BanType.Job)
            {
                toAdd.AddJobRange(b["roles"].EnumerateArray().Select(x => x.GetString()));
            }

            toReturn.Add(toAdd);
        }

        return toReturn;
    }

    public async Task<List<Ban>> GetBansBatchedAsync(int startpage = 1, int pages = -1)
    {
        var maxPages = await GetNumberOfPagesAsync();
        var range = Enumerable.Range(startpage, pages != -1 ? pages : maxPages + 1); // pad with a page for safety
        var toReturn = new ConcurrentBag<Ban>();
        var allTasks = new List<Task>();
        var throttle = new SemaphoreSlim(ParallelRequests);
        var requestsInPeriod = 0;
        var periodReset = DateTime.Now.AddMinutes(1);
            
        foreach (var page in range)
        {
            await throttle.WaitAsync();

            // Handle rate limiting
            if (requestsInPeriod >= RequestsPerMinute)
            {
                await Task.Delay(periodReset - DateTime.Now);
                periodReset = periodReset.AddMinutes(1);
                requestsInPeriod = 0;
            }
            requestsInPeriod++;

            // Fetch page
            allTasks.Add(Task.Run(async () =>
            {
                try
                {
                    foreach (var b in await GetBansAsync(page))
                    {
                        toReturn.Add(b);
                    }
                }
                finally
                {
                    throttle.Release();
                }
            }));
        }

        await Task.WhenAll(allTasks);

        return toReturn.ToList();
    }

    private async Task<int> GetNumberOfPagesAsync()
    {
        var content = await GetAsStringAsync("bans");
        var match = _pagesPattern.Matches(content);
        if (!match.Any())
        {
            throw new Exception("Failed to find page numbers on yogstation.net bans page");
        }

        // Yog has 20 bans per page by default, and we retrieve pages of 1000
        return (int)Math.Ceiling(int.Parse(match.Last().Groups["pagenum"].Value) * 20 / 1000.0);
    }
}