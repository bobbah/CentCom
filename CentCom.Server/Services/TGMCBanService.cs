using CentCom.Common.Models;
using CentCom.Server.Exceptions;
using CentCom.Server.Extensions;
using Extensions;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CentCom.Server.Services
{
    /// <summary>
    /// TGMC Ban Service for getting bans from the API
    /// </summary>
    /// <remarks>
    /// Note that the data is provided in a flat tg format, meaning
    /// that each role in a jobban has its own ban ID. Thus, our strategy
    /// for using the paging must account for the possibility of a job ban
    /// spanning two seperate pages.
    /// </remarks>
    public class TGMCBanService
    {
        private readonly IRestClient _client;
        private readonly ILogger _logger;
        private const string BASE_URL = "http://statbus.psykzz.com:8080/api/";
        private const int RECORDS_PER_PAGE = 100;
        private readonly static BanSource _banSource = new BanSource() { Name = "tgmc" };

        public TGMCBanService(ILogger<TGMCBanService> logger)
        {
            _logger = logger;
            _client = new RestClient(BASE_URL);
        }

        public async Task<IEnumerable<Ban>> GetBansAsync(int page = 1)
        {
            var request = new RestRequest($"bans/{page}", Method.GET, DataFormat.Json).AddQueryParameter("limit", RECORDS_PER_PAGE.ToString());
            var response = await _client.ExecuteAsync(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError($"TGMC website returned a non-200 HTTP response code: {response.StatusCode}, aborting parse.");
                throw new BanSourceUnavailableException($"TGMC website returned a non-200 HTTP response code: {response.StatusCode}, aborting parse.");
            }

            var toReturn = new List<Ban>();
            var dirtyBans = new List<Ban>();
            var content = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(response.Content);
            foreach (var bh in content["bans"].EnumerateObject())
            {
                var ban = bh.Value;

                // Ban expiration could be based on the expiration time field or the existance of the unbanned datetime
                // field, so we have to check both.
                var expiration = ban.GetProperty("unbanned_datetime").GetString() == null ? (DateTime?)null
                        : DateTime.Parse(ban.GetProperty("unbanned_datetime").GetString());
                if (!expiration.HasValue)
                {
                    expiration = ban.GetProperty("expiration_time").GetString() == null ? (DateTime?)null
                        : DateTime.Parse(ban.GetProperty("expiration_time").GetString());
                }

                // Get ban
                var toAdd = new Ban()
                {
                    BanID = bh.Name,
                    BannedBy = ban.GetProperty("admin").GetString(),
                    BannedOn = DateTime.Parse(ban.GetProperty("bantime").ToString()),
                    CKey = ban.GetProperty("ckey").GetString(),
                    Expires = expiration,
                    Reason = ban.GetProperty("reason").ToString(),
                    BanType = ban.GetProperty("role").GetString().ToLower() == "server" ? BanType.Server : BanType.Job,
                    SourceNavigation = _banSource
                };

                // Specify UTC
                toAdd.BannedOn = DateTime.SpecifyKind(toAdd.BannedOn, DateTimeKind.Utc);
                if (toAdd.Expires.HasValue)
                {
                    toAdd.Expires = DateTime.SpecifyKind(toAdd.Expires.Value, DateTimeKind.Utc);
                }

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
            foreach (var group in dirtyBans.GroupBy(x => new { x.BannedOn, x.CKey, x.Reason }))
            {
                var firstBan = group.OrderBy(x => x.BanID).First();
                firstBan.AddJobRange(group.SelectMany(x => x.JobBans).Select(x => x.Job));
                toReturn.Add(firstBan);
            }

            return toReturn;
        }

        public async Task<IEnumerable<Ban>> GetBansBatchedAsync(int startPage = 1, int pages = -1)
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

            // We have to ensure that our jobs are correctly grouped due to possible errors with paging
            var cleanBans = new List<Ban>(dirtyBans.Where(x => x.BanType == BanType.Server));
            foreach (var group in dirtyBans.Where(x => x.BanType == BanType.Job).GroupBy(x => new { x.BannedOn, x.CKey, x.Reason }))
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
            var request = new RestRequest($"bans/1", Method.GET, DataFormat.Json).AddQueryParameter("limit", RECORDS_PER_PAGE.ToString());
            var result = await _client.ExecuteAsync(request);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Unexpected non-200 status code [{result.StatusCode}] when trying to retrieve number of ban pages for TGMC");
            }

            var content = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(result.Content);
            if (content["page"].TryGetProperty("total", out var lastpage))
            {
                return lastpage.GetInt32();
            }
            else
            {
                throw new Exception("Failed to find the last page number in the response from TGMC's API");
            }
        }
    }
}
