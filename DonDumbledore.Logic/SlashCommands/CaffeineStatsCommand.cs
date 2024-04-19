using Discord;
using Discord.WebSocket;
using DonDumbledore.Logic.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace DonDumbledore.Logic.Requests;

public class CaffeineStatsCommand(IServiceProvider serviceProvider) : IDonCommand
{
    private const string NAME = "stats";
    private const string DESCRIPTION = "A személyes drogozási szokásaid vagy a bölcsek anyaga";
    private const string VALUE_OPTION = "üzenet";

    public string Name => NAME;

    public SlashCommandProperties CreateProperties()
    {
        var builder = new SlashCommandBuilder();

        builder.WithName(NAME);
        builder.WithDescription(DESCRIPTION);
        builder.AddOption(VALUE_OPTION, ApplicationCommandOptionType.String, "üzenet", false);
        return builder.Build();
    }

    public async Task Handle(SocketSlashCommand arg)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        await using var botDbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();


        if (arg.Data.Options.Count() == 0)
        {
            var count = await botDbContext.DrinkModels.CountAsync(x => x.UserId == arg.User.Id);
            var latests = await botDbContext
                .DrinkModels
                .Where(x => x.UserId == arg.User.Id)
                .OrderByDescending(x => x.CreatedAt)
                .Take(3)
                .Select(x => new { x.CreatedAt, x.Caffeine })
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
        else
        {
            var param = (string?)arg.Data.Options.FirstOrDefault().Value;
            var count = await botDbContext.MessageModels.CountAsync(x => x.UserId == arg.User.Id && x.MessageValue.Equals(param));
            var latests = await botDbContext
                .MessageModels
                .Where(x => x.UserId == arg.User.Id && x.MessageValue.Equals(param))
                .OrderByDescending(x => x.CreatedAt)
                .Take(3)
                .Select(x => new { x.CreatedAt })
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Legutóbbi 3 beszólása a kisfiúnak:");

            foreach (var latest in latests)
            {
                sb.AppendLine($"{latest.CreatedAt.ToString("yyyy-MM-dd @ HH:mm")} - {param}");
            }

            sb.AppendLine($"Összesen {count} alkalommal szólottál ilyet \"{param}\", de a Don még nem adott golyót");

            await arg.RespondAsync(sb.ToString());
        }
    }
}