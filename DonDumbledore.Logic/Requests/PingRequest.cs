using Discord.WebSocket;
using DonDumbledore.Logic.Attributes;

namespace DonDumbledore.Logic.Requests;

[SlashCommandInfo("ping", "ping", Option = "message")]
public class PingRequest(SocketSlashCommand myProperty) : RequestBase(myProperty)
{
}
