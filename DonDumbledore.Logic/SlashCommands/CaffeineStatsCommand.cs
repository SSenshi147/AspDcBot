using Discord;
using Discord.WebSocket;
using DonDumbledore.Logic.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace DonDumbledore.Logic.Requests;

public class CaffeineStatsCommand(IServiceProvider serviceProvider) : IDonCommand
{
    public string Name => NAME;

    private const string NAME = "stats";
    private const string DESCRIPTION = "A személyes drogozási szokásaid";

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

        var count = botDbContext.DrinkModels.Count(x => x.UserId == arg.User.Id);
        var latests = await botDbContext
            .DrinkModels
            .Where(x => x.UserId == arg.User.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Take(3)
            .Select(x => new
            {
                x.CreatedAt,
                x.Caffeine
            })
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Legutóbbi 3 drogozása a kisfiúnak:");

        foreach (var latest in latests)
        {
            sb.AppendLine($"{latest.CreatedAt.ToString("yyyy-MM-dd @ HH:mm")} - {latest.Caffeine.ToString()}");
        }

        sb.AppendLine($"Összesen {count} alkalommal drogoztál, de a Don még nem adott golyót");

        await arg.RespondAsync(sb.ToString());
    }
}