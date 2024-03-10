using AspDcBot.Data;
using Discord;
using Discord.WebSocket;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AspDcBot.Commands;

public class MessageReceivedNotification : INotification
{
    public required SocketMessage SocketMessage { get; init; }
}

public class MessageReceivedNotificationHandler(
    BotDbContext botDbContext) : INotificationHandler<MessageReceivedNotification>
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

        if (!await botDbContext.UserDataModels.AnyAsync(x => x.UserId == arg.Author.Id, cancellationToken))
        {
            var userModel = new UserData
            {
                UserId = arg.Author.Id,
                Mention = arg.Author.Mention,
                UserName = arg.Author.Username,
            };

            await botDbContext.UserDataModels.AddAsync(userModel, cancellationToken);
        }

        var model = new DrinkModel
        {
            MessageId = arg.Id,
            TextChannelId = arg.Channel.Id,
            UserId = arg.Author.Id,
        };

        if (coffees.Contains(arg.CleanContent))
        {
            model.Caffeine = CaffeineType.Coffee;
        }
        else if (teas.Contains(arg.CleanContent))
        {
            model.Caffeine = CaffeineType.Tea;
        }

        await botDbContext.AddAsync(model, cancellationToken);
        await botDbContext.SaveChangesAsync(cancellationToken);

        await arg.AddReactionAsync(OkEmote.Instance);
    }
}

public sealed class OkEmote : IEmote
{
    public string Name => "✅";

    private OkEmote()
    {
    }

    private static OkEmote? _instance;
    public static OkEmote Instance
    {
        get
        {
            _instance ??= new OkEmote();
            return _instance;
        }
    }
}
