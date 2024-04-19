using Discord;
using Discord.WebSocket;

namespace DonDumbledore.Logic.Requests;

public interface IDonCommand
{
    string Name
    {
        get;
    }

    SlashCommandProperties CreateProperties();
    Task Handle(SocketSlashCommand arg);
}