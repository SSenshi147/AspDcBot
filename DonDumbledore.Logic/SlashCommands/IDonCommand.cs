using Discord;
using Discord.WebSocket;

namespace DonDumbledore.Logic.Requests;

public interface IDonCommand
{
    SlashCommandProperties CreateProperties();
    Task Handle(SocketSlashCommand arg);
    string Name { get; }
}