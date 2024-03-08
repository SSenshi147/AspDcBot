using AspDcBot.Data;
using Discord.WebSocket;
using MediatR;

namespace AspDcBot.Commands;

public class MessageReceivedNotification : INotification
{
    public required SocketMessage SocketMessage { get; init; }
}

public class MessageReceivedNotificationHandler(IServiceProvider serviceProvider) : INotificationHandler<MessageReceivedNotification>
{
    private const string tatakaeve = ":tatakaeve:";
    private const string coffee = "☕";
    private const string tea = "🍵";

    private readonly string[] coffees = [tatakaeve, coffee];
    private readonly string[] teas = [tea];
    private readonly string[] drinks = [tatakaeve, coffee, tea];

    public async Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken)
    {
        var arg = notification.SocketMessage;

        if (arg.Author.IsBot) return;

        if (!drinks.Contains(arg.CleanContent)) return;

        using var scope = serviceProvider.CreateScope();
        using var botDbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

        var user = botDbContext.DrinkModels.SingleOrDefault(x => x.UserId == arg.Author.Id);

        if (user is null || user == default)
        {
            var model = await botDbContext.DrinkModels.AddAsync(new DrinkModel { UserId = arg.Author.Id }, cancellationToken);
            user = model.Entity;
        }

        if (coffees.Contains(arg.CleanContent))
        {
            user.CoffeCount++;
            await arg.Channel.SendMessageAsync($"{arg.Author.Username} eddig {user.CoffeCount} kávét fogyasztott el!");
        }
        else if (teas.Contains(arg.CleanContent))
        {
            user.TeaCount++;
            await arg.Channel.SendMessageAsync($"{arg.Author.Username} eddig {user.TeaCount} teát fogyasztott el!");
        }

        await botDbContext.SaveChangesAsync(cancellationToken);
    }
}
