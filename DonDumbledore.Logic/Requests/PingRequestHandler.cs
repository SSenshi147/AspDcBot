using MediatR;

namespace DonDumbledore.Logic.Requests;

public class PingRequestHandler : IRequestHandler<PingRequest>
{
    public async Task Handle(PingRequest request, CancellationToken cancellationToken)
    {
        var asd = request.Arg.Data.Options.SingleOrDefault(x => x.Name == "message") ?? throw new Exception();
        var asd2 = (string)asd?.Value ?? throw new Exception();

        await request.Arg.RespondAsync(text: $"pong: {asd2}");
    }
}
