using MediatR;

namespace DonDumbledore.Logic.Requests;

public class PingRequestHandler : RequestBaseHandler<PingRequest>
{
    protected override async Task HandleInternal(CancellationToken cancellationToken)
    {
        var asd = Arg.Data.Options.SingleOrDefault(x => x.Name == "message") ?? throw new Exception();
        var asd2 = (string)asd?.Value ?? throw new Exception();

        await Arg.RespondAsync(text: $"pong: {asd2}");
    }
}
