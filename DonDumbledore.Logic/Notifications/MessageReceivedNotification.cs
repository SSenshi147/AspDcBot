using Discord.WebSocket;
using MediatR;

namespace DonDumbledore.Logic.Notifications;

public class MessageReceivedNotification : INotification
{
    public required SocketMessage SocketMessage { get; init; }
}