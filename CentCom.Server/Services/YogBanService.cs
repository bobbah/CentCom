using CentCom.Common.Models;
using CentCom.Server.Exceptions;
using CentCom.Server.Extensions;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CentCom.Server.Services
{
    public class YogBanService
    {
        private const int _parallelRequests = 12;
        private const int _requestsPerMinute = 60;
        private readonly IRestClient _client;
        private const string BaseUrl = "https://yogstation.net/";
        private readonly Regex _pagesPattern = new Regex(@"<a class=""pagination-link[^>]+>(?<pagenum>[0-9]+)<\/a>", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly BanSource _source = new BanSource() { Name = "yogstation" };
        private readonly ILogger _logger;

        public YogBanService(ILogger<YogBanService> logger)
        {
            _client = new RestClient(BaseUrl);
            _logger = logger;
        }

        private async Task<IEnumerable<Ban>> GetBansAsync(int page = 1)
        {
            var request = new RestRequest("bans", Method.GET, DataFormat.Json)
                .AddQueryParameter("json", "1")
                .AddQueryParameter("page", page.ToString())
                .AddQueryParameter("amount", "1000");
            var response = await _client.ExecuteAsync(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError("Yogstation website returned a non-200 HTTP response code: {StatusCode}, aborting parse", response.StatusCode);
                throw new BanSourceUnavailableException($"Yogstation website returned a non-200 HTTP response code: {response.StatusCode}, aborting parse");
            }

            var toReturn = new List<Ban>();
            var content = JsonSerializer.Deserialize<IEnumerable<Dictionary<string, JsonElement>>>(response.Content);
            foreach (var b in content)
            {
                var expiryString = b["unbanned_datetime"].GetString() ?? b["expiration_time"].GetString();
                var toAdd = new Ban()
                {
                    BannedOn = DateTime.SpecifyKind(DateTime.Parse(b["bantime"].GetString()), DateTimeKind.Utc),
                    BannedBy = b["a_ckey"].GetString(),
                    BanType = b["roles"].EnumerateArray().Select(x => x.GetString()).Contains("Server")
                        ? BanType.Server
                        : BanType.Job,
                    Expires = expiryString == null ? null : DateTime.SpecifyKind(DateTime.Parse(expiryString), DateTimeKind.Utc),
                    CKey = b["ckey"].GetString(),
                    Reason = b["reason"].GetString(),
                    BanID = b["id"].GetInt32().ToString(),
                    SourceNavigation = _source
                };

                if (toAdd.BanType == BanType.Job)
                {
                    toAdd.AddJobRange(b["roles"].EnumerateArray().Select(x => x.GetString()));
                }

                toReturn.Add(toAdd);
            }

            return toReturn;
        }

        public async Task<IEnumerable<Ban>> GetBansBatchedAsync(int startpage = 1, int pages = -1)
        {
            var maxPages = await GetNumberOfPagesAsync();
            var range = Enumerable.Range(startpage, pages != -1 ? pages : maxPages + 1); // pad with a page for safety
            var toReturn = new ConcurrentBag<Ban>();
            var allTasks = new List<Task>();
            var throttle = new SemaphoreSlim(_parallelRequests);
            var requestsInPeriod = 0;
            var periodReset = DateTime.Now.AddMinutes(1);
            
            foreach (var page in range)
            {
                await throttle.WaitAsync();

                // Handle rate limiting
                if (requestsInPeriod >= _requestsPerMinute)
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

            return toReturn;
        }

        private async Task<int> GetNumberOfPagesAsync()
        {
            var request = new RestRequest("bans", Method.GET, DataFormat.None);
            var result = await _client.ExecuteAsync(request);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Unexpected non-200 status code [{result.StatusCode}] when trying to retrieve number of ban pages on yogstation.net");
            }

            var match = _pagesPattern.Matches(result.Content);
            if (!match.Any())
            {
                throw new Exception("Failed to find page numbers on yogstation.net bans page");
            }

            // Yog has 20 bans per page by default, and we retrieve pages of 1000
            return (int)Math.Ceiling(int.Parse(match.Last().Groups["pagenum"].Value) * 20 / 1000.0);
        }
    }
}
