using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CentCom.Common.Data;
using CentCom.Common.Models;
using CentCom.Common.Models.Byond;
using CentCom.Common.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace CentCom.Bot.Commands;

public class SearchCommands : CommandGroup
{
    private readonly DatabaseContext _dbContext;
    private readonly FeedbackService _feedback;

    public SearchCommands(DatabaseContext dbContext, FeedbackService feedback)
    {
        _dbContext = dbContext;
        _feedback = feedback;
    }

    [Command("search")]
    [Description("Search and view a summary for a player on CentCom")]
    public async Task<IResult> LookupCkey(string key)
    {
        var ckey = new CKey(key);
        var bans = (await _dbContext.Bans
                .Include(x => x.JobBans)
                .Include(x => x.SourceNavigation)
                .Where(x => x.CKey == ckey.CanonicalKey)
                .ToListAsync())
            .Select(BanData.FromBan);

        // Collect statistics
        var fields = new List<IEmbedField>
        {
            new EmbedField("Server bans", bans.Count(x => x.Type == BanType.Server).ToString(), true),
            new EmbedField("Job bans", bans.Count(x => x.Type == BanType.Job).ToString(), true),
            new EmbedField("Active bans", bans.Count(x => x.Active).ToString(), true)
        };

        // Generate embed to relay the information
        var embed = new Embed("Search Results",
            Colour: _feedback.Theme.Success,
            Description:
            $"Found the following details when searching for ``{key}``. You may be interested in viewing the " +
            $"full details of the bans on [CentCom](https://centcom.melonmesa.com/viewer/view/{key}), or viewing " +
            $"their /tg/ activity on [Scrubby](https://scrubby.melonmesa.com/ckey/{key}).",
            Fields: fields,
            Timestamp: DateTimeOffset.UtcNow,
            Footer: new EmbedFooter(VersionUtility.Version));

        return await _feedback.SendContextualEmbedAsync(embed);
    }
}