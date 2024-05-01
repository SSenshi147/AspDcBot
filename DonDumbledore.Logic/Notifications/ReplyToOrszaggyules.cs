using MediatR;

namespace DonDumbledore.Logic.Notifications;

public class ReplyToOrszaggyules : INotificationHandler<MessageReceivedNotification>
{
    private const string OGY = "országgyűlés";
    private readonly List<string> _replies = ["erre te is tudod a választ", "hát te jó hülye vagy", "kérdezd meg 2099-ben", "golyót akarsz?", ":KEKW:"];

    public async Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken)
    {
        if (!notification.SocketMessage.Content.Trim().ToLower().Contains(OGY) &&
            !notification.SocketMessage.CleanContent.Trim().ToLower().Contains(OGY))
            return;

        var random = new Random();
        var shouldReply = random.Next(2) == 0;
        if (!shouldReply) return;

        await notification.SocketMessage.Channel.SendMessageAsync(text: _replies[random.Next(_replies.Count)]);
    }
}