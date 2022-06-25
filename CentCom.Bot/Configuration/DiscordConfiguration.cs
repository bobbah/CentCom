namespace CentCom.Bot.Configuration;

public class DiscordConfiguration
{
    public string Token { get; set; }

    public ulong? FailureChannel { get; set; }

    public ulong? FailureMention { get; set; }
}