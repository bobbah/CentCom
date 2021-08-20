using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using CentCom.Common.Extensions;
using CentCom.Common.Models;
using Extensions;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace CentCom.Server.Services
{
    public class BeeBanService : RestBanService
    {
        private const int ParallelRequests = 1;
        private static readonly BanSource LrpSource = new BanSource() { Name = "bee-lrp" };
        private static readonly BanSource MrpSource = new BanSource() { Name = "bee-mrp" };

        public BeeBanService(ILogger<BeeBanService> logger) : base(logger)
        {
        }

        protected override string BaseUrl => "https://api.beestation13.com/";

        internal async Task<IEnumerable<Ban>> GetBansAsync(int page = 1)
        {
            var request =
                new RestRequest("bans", Method.GET, DataFormat.Json).AddQueryParameter("page", page.ToString());
            var response = await Client.ExecuteAsync(request);
            
            if (response.StatusCode != HttpStatusCode.OK)
            {
                FailedRequest(response);
            }

            var toReturn = new List<Ban>();
            var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
            foreach (var b in content.GetProperty("data").EnumerateArray())
            {
                var expiryString = b.GetProperty("unbanned_datetime").GetString() ??
                                   b.GetProperty("expiration_time").GetString();
                var toAdd = new Ban()
                {
                    BannedOn = DateTime.SpecifyKind(DateTime.Parse(b.GetProperty("bantime").GetString()),
                        DateTimeKind.Utc),
                    BannedBy = b.GetProperty("a_ckey").GetString(),
                    UnbannedBy = b.GetProperty("unbanned_ckey").GetString(),
                    BanType = b.GetProperty("roles").EnumerateArray().Select(x => x.GetString()).Contains("Server")
                        ? BanType.Server
                        : BanType.Job,
                    Expires = expiryString == null
                        ? null
                        : DateTime.SpecifyKind(DateTime.Parse(expiryString), DateTimeKind.Utc),
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
            }, ParallelRequests);
            return toReturn;
        }

        internal async Task<int> GetNumberOfPagesAsync()
        {
            var request = new RestRequest("bans", Method.GET, DataFormat.Json);
            var result = await Client.ExecuteAsync(request);

            if (result.StatusCode != HttpStatusCode.OK)
                FailedRequest(result);

            return JsonSerializer.Deserialize<JsonElement>(result.Content).GetProperty("pages").GetInt32();
        }

        private static BanSource ParseBanSource(string raw)
        {
            return (raw.ToLower()) switch
            {
                "bs_golden" => LrpSource,
                "bs_sage" => MrpSource,
                "bs_acacia" => MrpSource,
                _ => throw new Exception(
                    $"Failed to convert raw value of Beestation ban source to BanSource: \"{raw}\""),
            };
        }
    }
}