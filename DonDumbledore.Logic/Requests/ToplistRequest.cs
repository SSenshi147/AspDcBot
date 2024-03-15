using Discord.WebSocket;
using DonDumbledore.Logic.Attributes;

namespace DonDumbledore.Logic.Requests;

[SlashCommandInfo("toplist", "Az intézet bentlakóinak statisztikája")]
public class ToplistRequest(SocketSlashCommand myProperty) : RequestBase(myProperty)
{
}
