using Discord.WebSocket;
using MediatR;

namespace DonDumbledore.Logic.Requests;

public abstract class RequestBase(SocketSlashCommand arg) : IRequest
{
    public SocketSlashCommand Arg { get; } = arg;
}

public abstract class RequestBaseHandler<TRequest> : IRequestHandler<TRequest> where TRequest : RequestBase
{
    protected virtual SocketSlashCommand Arg { get; set; } = default!;

    public virtual async Task Handle(TRequest request, CancellationToken cancellationToken)
    {
        Arg = request.Arg;
        await HandleInternal(cancellationToken);
    }

    protected abstract Task HandleInternal(CancellationToken cancellationToken);
}
