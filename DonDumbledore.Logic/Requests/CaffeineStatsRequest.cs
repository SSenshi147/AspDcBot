using Discord.WebSocket;
using DonDumbledore.Logic.Attributes;

namespace DonDumbledore.Logic.Requests;

[SlashCommandInfo("stats", "A személyes drogozási szokásaid")]
public class CaffeineStatsRequest(SocketSlashCommand myProperty) : RequestBase(myProperty)
{
}
