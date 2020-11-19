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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CentCom.Server.Services
{
    public class BeeBanService
    {
        private readonly IRestClient _client;
        private readonly string _baseURL = "https://beestation13.com/";
        private readonly Regex _pagesPattern = new Regex("page [0-9]+ of (?<maxpages>[0-9]+)");
        private readonly static BanSource _lrpSource = new BanSource() { Name = "bee-lrp" };
        private readonly static BanSource _mrpSource = new BanSource() { Name = "bee-mrp" };
        private readonly ILogger _logger;

        public BeeBanService(ILogger<BeeBanService> logger)
        {
            _client = new RestClient(_baseURL);
            _logger = logger;
        }

        public async Task<IEnumerable<Ban>> GetBansAsync(int page = 1)
        {
            var request = new RestRequest("bans", Method.GET, DataFormat.Json).AddQueryParameter("json", "1").AddQueryParameter("page", page.ToString());
            var response = await _client.ExecuteAsync(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError($"Beestation website returned a non-200 HTTP response code: {response.StatusCode}, aborting parse.");
                throw new BanSourceUnavailableException($"Beestation website returned a non-200 HTTP response code: {response.StatusCode}, aborting parse.");
            }

            var toReturn = new List<Ban>();
            var content = JsonSerializer.Deserialize<IEnumerable<Dictionary<string, JsonElement>>>(response.Content);
            foreach (var b in content)
            {
                var toAdd = new Ban()
                {
                    BannedOn = DateTime.Parse(b["ban_date"].GetString()).ToUniversalTime(),
                    BannedBy = b["banner"].GetString(),
                    BanType = ParseBanType(b["type"].GetString()),
                    Expires = b["unban_date"].GetString() == null ? (DateTime?)null : DateTime.Parse(b["unban_date"].GetString()).ToUniversalTime(),
                    CKey = b["user"].GetString(),
                    Reason = b["reason"].GetString(),
                    BanID = b["id"].GetInt32().ToString(),
                    SourceNavigation = ParseBanSource(b["server"].GetString())
                };

                if (toAdd.BanType == BanType.Job)
                {
                    toAdd.AddJobRange(b["job"].EnumerateArray().Select(x => x.GetString()));
                }

                if (b["global"].GetBoolean())
                {
                    toAdd.AddAttribute(BanAttribute.BeeStationGlobal);
                }

                toReturn.Add(toAdd);
            }

            return toReturn;
        }

        public async Task<IEnumerable<Ban>> GetBansBatchedAsync(int startpage = 1, int pages = -1)
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
            }, 12);
            return toReturn;
        }

        public async Task<int> GetNumberOfPagesAsync()
        {
            var request = new RestRequest("bans", Method.GET, DataFormat.None);
            var result = await _client.ExecuteAsync(request);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Unexpected non-200 status code [{result.StatusCode}] when trying to retrieve number of ban pages on beestation13.com.");
            }

            var match = _pagesPattern.Match(result.Content);
            if (!match.Success)
            {
                throw new Exception("Failed to find page numbers on beestation13.com bans page");
            }
            else
            {
                return int.Parse(match.Groups["maxpages"].Value);
            }
        }

        private static BanType ParseBanType(string raw)
        {
            return (raw.ToLower()) switch
            {
                "server" => BanType.Server,
                "job" => BanType.Job,
                _ => throw new Exception($"Failed to convert raw value of Beestation ban to BanType: \"{raw}\""),
            };
        }

        private static BanSource ParseBanSource(string raw)
        {
            return (raw.ToLower()) switch
            {
                "bs_golden" => _lrpSource,
                "bs_sage" => _mrpSource,
                _ => throw new Exception($"Failed to convert raw value of Beestation ban source to BanSource: \"{raw}\""),
            };
        }
    }
}
