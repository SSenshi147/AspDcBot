using Discord;
using Discord.WebSocket;
using DonDumbledore.Logic.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace DonDumbledore.Logic.SlashCommands;

public class ToplistCommand(IServiceProvider serviceProvider) : IDonCommand
{
    private const string NAME = "toplist";
    private const string DESCRIPTION = "Az intézet bentlakóinak statisztikája";
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
        var message = (string?)arg.Data.Options.FirstOrDefault()?.Value;

        if (string.IsNullOrEmpty(message))
        {
            var result = await botDbContext
                .DrinkModels
                .GroupBy(x => x.UserId, (userId, models) => new { User = userId, Count = models.Count() })
                .Join(botDbContext.UserDataModels, arg => arg.User, data => data.UserId, (arg1, data) => new { arg1.Count, data.UserName })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            if (result.Count == 0)
            {
                await arg.RespondAsync("hát te még nem is drogoztál!");
                return;
            }

            var sb = new StringBuilder(result.Count);
            for (var i = 0; i < result.Count; i++)
            {
                var item = result[i];
                sb.AppendLine($"{i + 1}. {item.UserName} - {item.Count} drogozási alkalom");
            }

            await arg.RespondAsync(sb.ToString());
        }
        else
        {
            var result = await botDbContext
                .MessageModels
                .Where(x => x.MessageValue.Equals(message))
                .GroupBy(x => x.UserId, (userId, models) => new { User = userId, Count = models.Count() })
                .Join(botDbContext.UserDataModels, arg => arg.User, data => data.UserId, (arg1, data) => new { data.UserName, arg1.Count })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            if (result.Count == 0)
            {
                await arg.RespondAsync($"hát te még nem is szóltál be hogy {message}!");
                return;
            }

            var sb = new StringBuilder(result.Count);
            for (var i = 0; i < result.Count; i++)
            {
                var item = result[i];
                sb.AppendLine($"{i + 1}. {item.UserName} - {item.Count} beszólási alkalom evvel: {message}");
            }

            await arg.RespondAsync(sb.ToString());
        }
    }
}