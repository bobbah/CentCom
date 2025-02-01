using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AngleSharp;
using CentCom.Common.Extensions;
using CentCom.Common.Models;
using CentCom.Server.Exceptions;
using Microsoft.Extensions.Logging;

namespace CentCom.Server.Services;

public class VgBanService(ILogger<VgBanService> logger)
{
    private static readonly BanSource BanSource = new() { Name = "vgstation" };
    private readonly ILogger _logger = logger;

    // TODO: cleanup
    public async Task<List<Ban>> GetBansAsync()
    {
        var toReturn = new List<Ban>();
        var config = AngleSharp.Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync("https://ss13.moe/index.php/bans");

        if (document.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogError(
                "Source website returned a non-200 HTTP response code. Url: \"{Url}\", code: {StatusCode}", document.Url, document.StatusCode);
            throw new BanSourceUnavailableException(
                $"Source website returned a non-200 HTTP response code. Url: \"{document.Url}\", code: {document.StatusCode}", document.TextContent);
        }

        var tables = document.QuerySelectorAll("form > table > tbody");
        var banTable = tables[0];
        var jobTable = tables[1];

        for (var i = 1; i < banTable.Children.Length; i++)
        {
            var cursor = banTable.Children[i];
            var ckey = cursor.Children[0].Children[0].TextContent.Trim();
            DateTimeOffset date =
                cursor.Children[0].Children[0].GetAttribute("title") == "0000-00-00 00:00:00"
                    ? DateTime.MinValue
                    : DateTime.Parse(cursor.Children[0].Children[0].GetAttribute("title").Trim());
            var reason = cursor.Children[1].TextContent.Trim();
            var bannedBy = cursor.Children[2].TextContent.Trim();
            var expiresText = cursor.Children[3].TextContent.Trim();
            DateTimeOffset? expires = null;
            if (DateTime.TryParse(expiresText, out var d))
            {
                expires = d;
            }

            toReturn.Add(new Ban
            {
                CKey = ckey,
                BannedOn = date.UtcDateTime,
                BannedBy = bannedBy,
                Reason = reason,
                Expires = expires?.UtcDateTime,
                BanType = BanType.Server,
                SourceNavigation = BanSource
            });
        }

        for (var i = 1; i < jobTable.Children.Length; i++)
        {
            var cursor = jobTable.Children[i];
            var bannedDetails = cursor.Children[0];
            var ckey = bannedDetails.Children[0].TextContent.Trim();
            DateTimeOffset date = 
                cursor.Children[0].Children[0].GetAttribute("title") == "0000-00-00 00:00:00"
                    ? DateTime.MinValue
                    : DateTime.Parse(cursor.Children[0].Children[0].GetAttribute("title").Trim());
            var jobDetails = cursor.QuerySelector(".clmJobs");
            var jobs = jobDetails.QuerySelectorAll("a").Select(x => x.TextContent.Trim()).Distinct();
            var reason = cursor.Children[2].TextContent.Trim();
            var bannedBy = cursor.Children[3].TextContent.Trim();
            var expiresText = cursor.Children[4].TextContent.Trim();
            DateTimeOffset? expires = null;
            if (DateTime.TryParse(expiresText, out var d))
            {
                expires = d;
            }

            var toAdd = new Ban
            {
                CKey = ckey,
                BanType = BanType.Job,
                Reason = reason,
                BannedBy = bannedBy,
                BannedOn = date.UtcDateTime,
                Expires = expires?.UtcDateTime,
                SourceNavigation = BanSource
            };

            toAdd.AddJobRange(jobs);
            toReturn.Add(toAdd);
        }

        return toReturn;
    }
}