using Discord;
using Discord.WebSocket;
using DonDumbledore.Logic.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace DonDumbledore.Logic.Requests;

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
        using var scope = serviceProvider.CreateAsyncScope();
        using var botDbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

        if (arg.Data.Options.Count() == 0)
        {
            var result = await botDbContext
                .DrinkModels
                .GroupBy(x => x.UserId, (userId, models) => new { User = userId, Count = models.Count() })
                .Join(botDbContext.UserDataModels, arg => arg.User, data => data.UserId, (arg1, data) => new { data.Mention, arg1.Count })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var sb = new StringBuilder(result.Count);
            for (var i = 0; i < result.Count; i++)
            {
                var item = result[i];
                sb.AppendLine($"{i + 1}. {item.Mention} - {item.Count} drogozási alkalom");
            }

            await arg.RespondAsync(sb.ToString());
        }
        else
        {
            var message = (string?)arg.Data.Options.FirstOrDefault().Value;
            var result = await botDbContext
                .MessageModels
                .Where(x => x.MessageValue.Equals(message))
                .GroupBy(x => x.UserId, (userId, models) => new { User = userId, Count = models.Count() })
                .Join(botDbContext.UserDataModels, arg => arg.User, data => data.UserId, (arg1, data) => new { data.Mention, arg1.Count })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var sb = new StringBuilder(result.Count);
            for (var i = 0; i < result.Count; i++)
            {
                var item = result[i];
                sb.AppendLine($"{i + 1}. {item.Mention} - {item.Count} beszólási alkalom evvel: {message}");
            }

            await arg.RespondAsync(sb.ToString());
        }
    }
}