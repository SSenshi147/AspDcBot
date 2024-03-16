using Discord;
using Discord.WebSocket;
using DonDumbledore.Logic.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace DonDumbledore.Logic.Requests;

public class ToplistCommand(BotDbContext botDbContext) : IDonCommand
{
    public string Name => NAME;

    private const string NAME = "toplist";
    private const string DESCRIPTION = "Az intézet bentlakóinak statisztikája";

    public SlashCommandProperties CreateProperties()
    {
        var builder = new SlashCommandBuilder();

        builder.WithName(NAME);
        builder.WithDescription(DESCRIPTION);

        return builder.Build();
    }

    public async Task Handle(SocketSlashCommand arg)
    {
        var result = await botDbContext
            .DrinkModels
            .GroupBy(x => x.UserId, (userId, models) => new
            {
                User = userId,
                Count = models.Count()
            })
            .Join(botDbContext.UserDataModels, arg => arg.User, data => data.UserId, (arg1, data) => new
            {
                data.Mention,
                arg1.Count
            })
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
}