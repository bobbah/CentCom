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
using System.Threading.Tasks;

namespace CentCom.Server.Services
{
    public class BeeBanService
    {
        private const int _parallelRequests = 12;
        private readonly IRestClient _client;
        private const string BaseUrl = "https://api.beestation13.com/";
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
            var request = new RestRequest("bans", Method.GET, DataFormat.Json).AddQueryParameter("page", page.ToString());
            var response = await _client.ExecuteAsync(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError("Beestation website returned a non-200 HTTP response code: {StatusCode}, aborting parse", response.StatusCode);
                throw new BanSourceUnavailableException($"Beestation website returned a non-200 HTTP response code: {response.StatusCode}, aborting parse");
            }

            var toReturn = new List<Ban>();
            var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
            foreach (var b in content.GetProperty("data").EnumerateArray())
            {
                var expiryString = b.GetProperty("unbanned_datetime").GetString() ?? b.GetProperty("expiration_time").GetString();
                var toAdd = new Ban()
                {
                    BannedOn = DateTime.SpecifyKind(DateTime.Parse(b.GetProperty("bantime").GetString()), DateTimeKind.Utc),
                    BannedBy = b.GetProperty("a_ckey").GetString(),
                    UnbannedBy = b.GetProperty("unbanned_ckey").GetString(),
                    BanType = b.GetProperty("roles").EnumerateArray().Select(x => x.GetString()).Contains("Server")
                        ? BanType.Server
                        : BanType.Job,
                    Expires = expiryString == null ? null : DateTime.SpecifyKind(DateTime.Parse(expiryString), DateTimeKind.Utc),
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
            var request = new RestRequest("bans", Method.GET, DataFormat.Json);
            var result = await _client.ExecuteAsync(request);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Unexpected non-200 status code [{result.StatusCode}] when trying to retrieve number of ban pages on beestation13.com");
            }

            return JsonSerializer.Deserialize<JsonElement>(result.Content).GetProperty("pages").GetInt32();
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
