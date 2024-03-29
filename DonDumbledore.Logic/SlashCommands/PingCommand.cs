using Discord;
using Discord.WebSocket;
using Hangfire;

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
        await arg.RespondAsync(text: "received");
        
        var neved = arg.Data.Options.FirstOrDefault()?.Value;
        
        await arg.FollowupAsync(text: $"pong {neved}");

        if (neved is not string asd) return;
        var timing = DateTime.TryParse(asd, out var time);
        if (!timing) return;
        Console.WriteLine("timing ok!");
        await arg.FollowupAsync(text: "processed");
        BackgroundJob.Schedule(() => Anyad(arg), DateTime.Parse(asd));
    }

    public async Task Anyad(SocketSlashCommand arg)
    {
        await arg.FollowupAsync(text: "asd");
    }
}