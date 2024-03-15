using MediatR;

namespace DonDumbledore.Logic.Requests;

public class PingRequestHandler : IRequestHandler<PingRequest>
{
    public async Task Handle(PingRequest request, CancellationToken cancellationToken)
    {
        await request.Arg.RespondAsync("pong");
    }
}
