using Discord.WebSocket;
using Discord;
using DonDumbledore.Logic.Data;
using DonDumbledore.Logic.Requests;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace DonDumbledore.Logic.SlashCommands;

public class TracklistCommand(IServiceProvider serviceProvider) : IDonCommand
{
    public string Name => NAME;

    private const string NAME = "tracklist";
    private const string DESCRIPTION = "A trekkelt üzeneteket mutatja meg";
    public SlashCommandProperties CreateProperties()
    {
        var builder = new SlashCommandBuilder();

        builder.WithName(NAME);
        builder.WithDescription(DESCRIPTION);
        return builder.Build();
    }

    public async Task Handle(SocketSlashCommand arg)
    {
        using var scope = serviceProvider.CreateAsyncScope();
        using var botDbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();


        var result = await botDbContext.TrackedMessageModels.ToListAsync();

        var sb = new StringBuilder(result.Count);
        for (var i = 0; i < result.Count; i++)
        {
            var item = result[i];
            sb.AppendLine($"{i + 1}. {item.MessageValue}");
        }

        await arg.RespondAsync(sb.ToString());
    }
}