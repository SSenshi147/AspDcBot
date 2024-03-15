using System.Text;
using DonDumbledore.Logic.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DonDumbledore.Logic.Requests;

public class CaffeineStatsRequestHandler(BotDbContext botDbContext) : IRequestHandler<CaffeineStatsRequest>
{
    public async Task Handle(CaffeineStatsRequest request, CancellationToken cancellationToken)
    {
        var args = request.Arg;

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
