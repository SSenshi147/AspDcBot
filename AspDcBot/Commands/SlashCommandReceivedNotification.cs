using Discord.WebSocket;
using MediatR;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AspDcBot.Commands;

public class PingCommand : SlashRequest
{
    public override string Name => "ping";

    public override string Description => "ping";
}

public abstract class SlashRequest : IRequest
{
    public abstract string Name { get; }
    public abstract string Description { get; }

    public required SocketSlashCommand Args { get; init; }
}

public class PingCommandHandler : IRequestHandler<PingCommand>
{
    public async Task Handle(PingCommand request, CancellationToken cancellationToken)
    {
        await request.Args.RespondAsync(text: "ping received");
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class SlashCommandInfoAttribute : Attribute
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}

public sealed class TestNoti : INotification
{
    public required SocketSlashCommand Args { get; init; }
}


public abstract class Test : INotificationHandler<TestNoti>
{
    public abstract string CommandName { get; }
    public abstract string CommandDescription { get; }

    public async Task Handle(TestNoti notification, CancellationToken cancellationToken)
    {
        if (notification.Args.CommandName != CommandName)
        {
            return;
        }

        await Do(notification, cancellationToken);
    }

    protected abstract Task Do(TestNoti notification, CancellationToken cancellationToken);
}

[SlashCommandInfo(Description = DESC, Name = NAME)]
public class PingHandler : Test
{
    public const string NAME = "ping";
    public override string CommandName => NAME;

    public const string DESC = "ping desc";
    public override string CommandDescription => DESC;

    protected override async Task Do(TestNoti notification, CancellationToken cancellationToken)
    {
        await notification.Args.RespondAsync(text: "pong");
    }
}

[SlashCommandInfo(Description = DESC, Name = NAME)]
public class CicaHandler : Test
{
    public const string NAME = "cica";
    public override string CommandName => NAME;

    public const string DESC = "cica desc";
    public override string CommandDescription => DESC;

    protected override async Task Do(TestNoti notification, CancellationToken cancellationToken)
    {
        await notification.Args.RespondAsync(text: "meow");
    }
}