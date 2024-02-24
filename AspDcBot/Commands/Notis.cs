using Discord.WebSocket;
using MediatR;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AspDcBot.Commands;

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