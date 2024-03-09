using System.Text;
using AspDcBot.Data;
using Discord.Interactions;
using Discord.WebSocket;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AspDcBot.Commands;

public abstract class Requests(SocketSlashCommand myProperty) : IRequest
{
    public SocketSlashCommand MyProperty { get; } = myProperty;
}

[SlashCommandInfo("ping", "ping")]
public class PingRequest(SocketSlashCommand myProperty) : Requests(myProperty)
{
}

public class PingRequestHandler : IRequestHandler<PingRequest>
{
    public async Task Handle(PingRequest request, CancellationToken cancellationToken)
    {
        await request.MyProperty.RespondAsync(text: "pong");
    }
}

[SlashCommandInfo("stats", "A személyes drogozási szokásaid")]
public class CaffeineStatsRequest(SocketSlashCommand myProperty) : Requests(myProperty)
{
}

public class CaffeineStatsRequestHandler(BotDbContext botDbContext) : IRequestHandler<CaffeineStatsRequest>
{
    public async Task Handle(CaffeineStatsRequest request, CancellationToken cancellationToken)
    {
        var args = request.MyProperty;

        var count = botDbContext.DrinkModels.Count(x => x.UserId == args.User.Id);
        var latests = await botDbContext
            .DrinkModels
            .Where(x => x.UserId == args.User.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Take(3)
            .Select(x => new
            {
                x.CreatedAt,
                x.Caffeine
            })
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Legutóbbi 3 drogozása a kisfiúnak:");
        foreach (var latest in latests)
        {
            sb.AppendLine($"{latest.CreatedAt.ToString("yyyy-MM-dd @ HH:mm")} - {latest.Caffeine.ToString()}");
        }

        sb.AppendLine($"Összesen {count} alkalommal drogoztál, de a Don még nem adott golyót");

        await args.RespondAsync(sb.ToString());
    }
}

[SlashCommandInfo("toplist", "Az intézet bentlakóinak statisztikája")]
public class ToplistRequest(SocketSlashCommand myProperty) : Requests(myProperty)
{
}

public class ToplistRequestHandler(BotDbContext botDbContext) : IRequestHandler<ToplistRequest>
{
    public async Task Handle(ToplistRequest request, CancellationToken cancellationToken)
    {
        var args = request.MyProperty;

        var result = await botDbContext
            .DrinkModels
            .GroupBy(x => x.UserId, (userId, models) => new
            {
                User = userId,
                Count = models.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder(result.Count);
        for (int i = 0; i < result.Count; i++)
        {
            var item = result[i];
            sb.AppendLine($"{i + 1}. {item.User} - {item.Count} drogozási alkalom");
        }

        await args.RespondAsync(sb.ToString());
    }
}