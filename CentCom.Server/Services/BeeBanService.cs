using CentCom.Common.Extensions;
using CentCom.Common.Models;
using CentCom.Server.Exceptions;
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
        private const int _parallelRequests = 12;
        private readonly IRestClient _client;
        private const string BaseUrl = "https://beestation13.com/";
        private readonly Regex _pagesPattern = new Regex("page [0-9]+ of (?<maxpages>[0-9]+)", RegexOptions.Compiled);
        private static readonly BanSource LrpSource = new BanSource() { Name = "bee-lrp" };
        private static readonly BanSource MrpSource = new BanSource() { Name = "bee-mrp" };
        private readonly ILogger _logger;

        public BeeBanService(ILogger<BeeBanService> logger)
        {
            _client = new RestClient(BaseUrl);
            _logger = logger;
        }

        private async Task<IEnumerable<Ban>> GetBansAsync(int page = 1)
        {
            var request = new RestRequest("bans", Method.GET, DataFormat.Json).AddQueryParameter("json", "1").AddQueryParameter("page", page.ToString());
            var response = await _client.ExecuteAsync(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError("Beestation website returned a non-200 HTTP response code: {StatusCode}, aborting parse", response.StatusCode);
                throw new BanSourceUnavailableException($"Beestation website returned a non-200 HTTP response code: {response.StatusCode}, aborting parse");
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
                    UnbannedBy = b["unbanned_ckey"].GetString(),
                    BanType = b["roles"].EnumerateArray().Select(x => x.GetString()).Contains("Server")
                        ? BanType.Server
                        : BanType.Job,
                    Expires = expiryString == null ? null : DateTime.SpecifyKind(DateTime.Parse(expiryString), DateTimeKind.Utc),
                    CKey = b["ckey"].GetString(),
                    Reason = b["reason"].GetString(),
                    BanID = b["id"].GetInt32().ToString(),
                    SourceNavigation = ParseBanSource(b["server_name"].GetString())
                };

                if (toAdd.BanType == BanType.Job)
                {
                    toAdd.AddJobRange(b["roles"].EnumerateArray().Select(x => x.GetString()));
                }

                if (b["global_ban"].GetInt32() == 1)
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
            }, _parallelRequests);
            return toReturn;
        }

        private async Task<int> GetNumberOfPagesAsync()
        {
            var request = new RestRequest("bans", Method.GET, DataFormat.None);
            var result = await _client.ExecuteAsync(request);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Unexpected non-200 status code [{result.StatusCode}] when trying to retrieve number of ban pages on beestation13.com");
            }

            var match = _pagesPattern.Match(result.Content);
            if (!match.Success)
            {
                throw new Exception("Failed to find page numbers on beestation13.com bans page");
            }

            return int.Parse(match.Groups["maxpages"].Value);
        }

        private static BanSource ParseBanSource(string raw)
        {
            return (raw.ToLower()) switch
            {
                "bs_golden" => LrpSource,
                "bs_sage" => MrpSource,
                "bs_acacia" => MrpSource,
                _ => throw new Exception($"Failed to convert raw value of Beestation ban source to BanSource: \"{raw}\""),
            };
        }
    }
}
