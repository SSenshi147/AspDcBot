using Discord.WebSocket;
using MediatR;

namespace DonDumbledore.Logic.Requests;

public abstract class RequestBase(SocketSlashCommand arg) : IRequest
{
    public SocketSlashCommand Arg { get; } = arg;
}
