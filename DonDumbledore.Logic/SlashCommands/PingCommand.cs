using Discord;
using Discord.WebSocket;

namespace DonDumbledore.Logic.SlashCommands;

public class PingCommand : IDonCommand
{
    private const string NAME_OPTION = "name";
    private const string NAME = "ping";
    private const string DESCRIPTION = "ping description";
    public string Name => NAME;

    public SlashCommandProperties CreateProperties()
    {
        var builder = new SlashCommandBuilder();

        builder.WithName(NAME);
        builder.WithDescription(DESCRIPTION);
        builder.AddOption(NAME_OPTION, ApplicationCommandOptionType.String, "neved", false);

        return builder.Build();
    }

    public async Task Handle(SocketSlashCommand arg)
    {
        await arg.RespondAsync($"pong {arg.Data.Options.FirstOrDefault()?.Value}");
    }
}