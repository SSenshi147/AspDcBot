using Discord;
using DonDumbledore.Logic.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DonDumbledore.Logic.Notifications;

public class MessageReceivedNotificationHandler(
    IServiceProvider serviceProvider) : INotificationHandler<MessageReceivedNotification>
{
    private const string tatakaeve = ":tatakaeve:";
    private const string coffee = "☕";
    private const string tea = "🍵";

    private readonly string[] coffees = [tatakaeve, coffee];
    private readonly string[] drinks = [tatakaeve, coffee, tea];
    private readonly string[] teas = [tea];

    public async Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken)
    {
        var arg = notification.SocketMessage;

        if (arg.Author.IsBot)
        {
            return;
        }

        using var scope = serviceProvider.CreateAsyncScope();
        using var botDbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

        var message = await botDbContext.TrackedMessageModels.FirstOrDefaultAsync(x => x.MessageValue.Equals(arg.CleanContent));

        if (message == null && !drinks.Contains(arg.CleanContent))
        {
            return;
        }

        if (drinks.Contains(arg.CleanContent))
        {
            if (!await botDbContext.UserDataModels.AnyAsync(x => x.UserId == arg.Author.Id, cancellationToken))
            {
                var userModel = new UserData { UserId = arg.Author.Id, Mention = arg.Author.Mention, UserName = arg.Author.Username };

                await botDbContext.UserDataModels.AddAsync(userModel, cancellationToken);
            }

            var model = new DrinkModel { MessageId = arg.Id, TextChannelId = arg.Channel.Id, UserId = arg.Author.Id };

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
        else if (message != null)
        {
            if (!await botDbContext.UserDataModels.AnyAsync(x => x.UserId == arg.Author.Id, cancellationToken))
            {
                var userModel = new UserData { UserId = arg.Author.Id, Mention = arg.Author.Mention, UserName = arg.Author.Username };

                await botDbContext.UserDataModels.AddAsync(userModel, cancellationToken);
            }

            var model = new MessageModel { MessageId = arg.Id, TextChannelId = arg.Channel.Id, UserId = arg.Author.Id, MessageValue = arg.CleanContent };


            await botDbContext.AddAsync(model, cancellationToken);
            await botDbContext.SaveChangesAsync(cancellationToken);

            await arg.AddReactionAsync(OkEmote.Instance);
        }
    }
}

public sealed class OkEmote : IEmote
{
    private static OkEmote? _instance;

    private OkEmote()
    {
    }

    public static OkEmote Instance
    {
        get
        {
            _instance ??= new OkEmote();
            return _instance;
        }
    }

    public string Name => "✅";
}