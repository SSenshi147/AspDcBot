using Discord;
using Discord.WebSocket;
using DonDumbledore.Logic.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace DonDumbledore.Logic.SlashCommands;

public class TracklistCommand(IServiceProvider serviceProvider) : IDonCommand
{
    private const string NAME = "tracklist";
    private const string DESCRIPTION = "A trekkelt üzeneteket mutatja meg";
    public string Name => NAME;

    public SlashCommandProperties CreateProperties()
    {
        var builder = new SlashCommandBuilder();

        builder.WithName(NAME);
        builder.WithDescription(DESCRIPTION);
        return builder.Build();
    }

    public async Task Handle(SocketSlashCommand arg)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        await using var botDbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

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