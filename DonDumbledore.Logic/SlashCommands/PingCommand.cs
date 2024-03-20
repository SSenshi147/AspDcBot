using Discord;
using Discord.WebSocket;
using DonDumbledore.Logic.Extensions;

namespace DonDumbledore.Logic.Requests;

public class PingCommand : IDonCommand
{
    public string Name => NAME;
    
    private const string NAME_OPTION = "name";
    private const string NAME = "ping";
    private const string DESCRIPTION = "ping description";

    public SlashCommandProperties CreateProperties()
    {
        var builder = new SlashCommandBuilder();

        builder.WithName(NAME);
        builder.WithDescription(DESCRIPTION);
        builder.AddOption(NAME_OPTION, ApplicationCommandOptionType.String, "neved", isRequired: false);

        return builder.Build();
    }

    public async Task Handle(SocketSlashCommand arg)
    {
        await arg.RespondAsync(text: $"pong {arg.GetDataByName(NAME)}");
    }
}