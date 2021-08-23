using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace CentCom.Bot.Responders
{
    public class ServerJoinResponder : IResponder<IGuildCreate>
    {
        private readonly ILogger _logger;
        private readonly SlashService _slash;

        public ServerJoinResponder(ILogger<ServerJoinResponder> logger, SlashService slash)
        {
            _slash = slash;
            _logger = logger;
        }

        public async Task<Result> RespondAsync(IGuildCreate gatewayEvent,
            CancellationToken ct = new CancellationToken())
        {
            var slashSupport = _slash.SupportsSlashCommands();
            if (!slashSupport.IsSuccess)
            {
                _logger.LogWarning("The registered commands of the bot don't support slash commands: {Reason}",
                    slashSupport.Error?.Message);
                return Result.FromError(slashSupport.Error);
            }
            
            var update = await _slash.UpdateSlashCommandsAsync(gatewayEvent.ID, ct);
            if (!update.IsSuccess)
            {
                _logger.LogWarning("Failed to update slash commands: {Reason}", update.Error?.Message);
                return Result.FromError(update.Error);
            }

            return Result.FromSuccess();
        }
    }
}