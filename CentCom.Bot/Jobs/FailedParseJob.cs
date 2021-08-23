using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentCom.Bot.Configuration;
using CentCom.Common.Data;
using CentCom.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;

namespace CentCom.Bot.Jobs
{
    [DisallowConcurrentExecution]
    public class FailedParseJob : IJob
    {
        private readonly IDiscordRestChannelAPI _channelAPI;
        private readonly IOptions<DiscordConfiguration> _config;
        private readonly DatabaseContext _dbContext;

        public FailedParseJob(DatabaseContext dbContext, IOptions<DiscordConfiguration> config,
            IDiscordRestChannelAPI channelAPI)
        {
            _dbContext = dbContext;
            _channelAPI = channelAPI;
            _config = config;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var failures = await _dbContext.CheckHistory
                .Include(x => x.Notification)
                .Where(x => !x.Success && x.Notification == null)
                .ToListAsync();
            if (failures.Count == 0)
                return;

            if (_config.Value == null)
                throw new Exception("Missing or invalid Discord configuration, cannot dispatch failure notifications");

            // Don't bother if we haven't configured a channel to use
            if (_config.Value.FailureChannel == null)
                return;

            // Get channel, check it exists
            var channelRequest = await _channelAPI.GetChannelAsync(new Snowflake(_config.Value.FailureChannel.Value));
            if (!channelRequest.IsSuccess || channelRequest.Entity == null)
                throw new Exception("Failed to get Discord channel to dispatch parse failure notifications into.");

            var notified = new List<NotifiedFailure>();
            var channel = channelRequest.Entity;
            foreach (var failure in failures)
            {
                // Attach text content of response where available
                FileData fileData = null;
                var messageSuffix = "The content of the response was missing or empty.";
                if (failure.ResponseContent != null)
                {
                    var dataStream = new MemoryStream();
                    var writer = new StreamWriter(dataStream);
                    await writer.WriteAsync(failure.ResponseContent);
                    dataStream.Seek(0, SeekOrigin.Begin);
                    fileData = new FileData("response_content.txt", dataStream);
                    messageSuffix = "The content of the response is attached to this message.";
                }
                
                var message = new StringBuilder();
                if (_config.Value.FailureMention.HasValue)
                    message.Append($"<@{_config.Value.FailureMention}> ");
                message.Append(
                    $"Failed to parse bans for {failure.Parser} at <t:{failure.Failed.Value.ToUnixTimeSeconds()}>, exception is as follows... ```");

                // Ensure that our length fits
                var currLength = message.Length + failure.Exception.Length + messageSuffix.Length + 3;
                message.Append(currLength > 2000
                    ? $"{failure.Exception[0..^(currLength - 2000 + 4)]}...```"
                    : $"{failure.Exception}```");

                // Add suffix
                message.Append(messageSuffix);

                // Try to send, only mark completed if successful
                var result = await _channelAPI.CreateMessageAsync(channel.ID, message.ToString(),
                    file: fileData ?? new Optional<FileData>());
                if (result.IsSuccess)
                    notified.Add(new NotifiedFailure()
                    {
                        CheckHistory = failure,
                        Timestamp = DateTimeOffset.UtcNow
                    });
            }

            _dbContext.NotifiedFailures.AddRange(notified);
            await _dbContext.SaveChangesAsync();
        }
    }
}