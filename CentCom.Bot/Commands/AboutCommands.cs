using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CentCom.Common.Data;
using Microsoft.EntityFrameworkCore;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace CentCom.Bot.Commands;

public class AboutCommands : CommandGroup
{
    private readonly DatabaseContext _dbContext;
    private readonly FeedbackService _feedback;

    public AboutCommands(DatabaseContext dbContext, FeedbackService feedback)
    {
        _dbContext = dbContext;
        _feedback = feedback;
    }

    [Command("about")]
    [Description("Get basic information about CentCom and its performance")]
    [CommandType(ApplicationCommandType.ChatInput)]
    public async Task<IResult> GetAboutAsync()
    {
        var stats = new List<IEmbedField>
        {
            new EmbedField("Total Bans", (await _dbContext.Bans.CountAsync()).ToString(), true),
            new EmbedField("Total Sources", (await _dbContext.BanSources.CountAsync()).ToString(), true),
            new EmbedField("Bans in Last 24h", (await _dbContext.Bans
                    .Where(x => x.BannedOn > DateTime.UtcNow.AddDays(-1))
                    .CountAsync()).ToString(), true
            )
        };
        var embed = new Embed("Oh, woah, what's this?",
            Description:
            "[CentCom](https://centcom.melonmesa.com/) is a ban data aggregation service for Space Station 13. " +
            "This bot, ``CentCom.Bot``, serves as a utility to provide feedback when something is wrong. You can " +
            "find the source on GitHub [here](https://github.com/bobbahbrown/centcom).",
            Fields: stats,
            Colour: _feedback.Theme.Success,
            Timestamp: DateTimeOffset.UtcNow,
            Footer: new EmbedFooter(VersionUtility.Version));
        var result = await _feedback.SendContextualEmbedAsync(embed, ct: CancellationToken);
        return result.IsSuccess ? Result.FromSuccess() : Result.FromError(result);
    }

    [Command("invite")]
    [Description("Get an invite link for the bot to your server")]
    [CommandType(ApplicationCommandType.ChatInput)]
    public async Task<IResult> GetInviteAsync()
    {
        const string inviteLink =
            "https://discord.com/api/oauth2/authorize?client_id=878121825333293127&permissions=51264&scope=bot%20applications.commands";
        return await _feedback.SendContextualEmbedAsync(new Embed(
            Description: $"[Click here]({inviteLink}) to invite me to your server!",
            Colour: _feedback.Theme.Primary,
            Timestamp: DateTimeOffset.UtcNow,
            Footer: new EmbedFooter(VersionUtility.Version)));
    }
}