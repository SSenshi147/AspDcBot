using Discord.WebSocket;

namespace DonDumbledore.Logic.Extensions;

public static class DiscordExtensions
{
    public static object? GetDataByName(this SocketSlashCommand command, string name)
    {
        return command.Data.Options.SingleOrDefault(x => x.Name == name)?.Value;
    }
}
