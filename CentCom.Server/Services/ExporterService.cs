using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CentCom.Common.Abstract;
using CentCom.Common.Extensions;
using CentCom.Common.Models;
using CentCom.Common.Models.Rest;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Extensions;
using RestSharp;
using RestSharp.Serializers.SystemTextJson;

namespace CentCom.Server.Services
{
    public class ExporterService : RestBanService
    {
        private static readonly BanSource BanSource = new BanSource { Name = "teststation" };

        public ExporterService(ILogger<ExporterService> logger) : base(logger)
        {
            var options = new JsonSerializerOptions();
            options.AddCentComOptions();
            options.AddDataObjectConverter<IRestBan, RestBan>();
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.Converters.Insert(0, new JsonStringEnumConverter());
            Client.UseSystemTextJson(options);
        }

        protected override string BaseUrl => "https://localhost:6658/";

        private async Task<IEnumerable<Ban>> GetBansAsync(int? cursor = null)
        {
            var request = new RestRequest("api/ban", Method.GET, DataFormat.Json);
            if (cursor.HasValue)
                request.AddQueryParameter("cursor", cursor.ToString());
            var response = await Client.ExecuteAsync<IEnumerable<IRestBan>>(request);

            if (response.StatusCode != HttpStatusCode.OK)
                FailedRequest(response);

            return response.Data.Select(x => new Ban
            {
                BanID = x.Id.ToString(),
                BannedBy = x.BannedBy?.CanonicalKey,
                BannedOn = x.BannedOn.DateTime,
                BanType = x.BanType,
                CKey = x.CKey.CanonicalKey,
                Expires = x.Expires?.DateTime,
                UnbannedBy = x.UnbannedBy?.CanonicalKey,
                Reason = x.Reason,
                JobBans = x.JobBans?.Select(j => new JobBan()
                    {
                        Job = j.Job
                    })
                    .ToHashSet(),
                SourceNavigation = BanSource
            });
        }

        public async Task<IEnumerable<Ban>> GetBansBatchedAsync(int? cursor = null, IEnumerable<int> searchFor = null)
        {
            // Get bans, must use a sequential approach here due to the last ban
            // ID only being available once we finish a page
            var result = new List<Ban>();
            var lastRequested = cursor;
            List<Ban> lastResponse;
            do
            {
                lastResponse = (await GetBansAsync(lastRequested)).ToList();
                if (!lastResponse.Any())
                    break;
                lastRequested = int.Parse(lastResponse[^1].BanID);
                result.AddRange(lastResponse);
            } while (lastResponse.Any() &&
                     (searchFor == null || lastResponse.Any(x => searchFor.Contains(int.Parse(x.BanID)))));

            return result;
        }
    }
}