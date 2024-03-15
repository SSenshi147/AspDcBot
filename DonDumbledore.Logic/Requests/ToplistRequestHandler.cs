using System.Text;
using DonDumbledore.Logic.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DonDumbledore.Logic.Requests;

public class ToplistRequestHandler(BotDbContext botDbContext) : IRequestHandler<ToplistRequest>
{
    public async Task Handle(ToplistRequest request, CancellationToken cancellationToken)
    {
        var args = request.Arg;

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
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder(result.Count);
        for (var i = 0; i < result.Count; i++)
        {
            var item = result[i];
            sb.AppendLine($"{i + 1}. {item.Mention} - {item.Count} drogozási alkalom");
        }

        await args.RespondAsync(sb.ToString());
    }
}