using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MediatR;

namespace AspDcBot.Commands;

public abstract class Requests(SocketSlashCommand myProperty) : IRequest
{
    public SocketSlashCommand MyProperty { get; } = myProperty;
}

[SlashCommandInfo(Description = "desc", Name = "asdasd")]
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