using Discord;
using Discord.WebSocket;
using DonDumbledore.Logic.Notifications;
using DonDumbledore.Logic.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DonDumbledore.Logic.Services;

public class DiscordBotService(
    DiscordSocketClient client,
    IMediator mediator,
    ILogger<DiscordBotService> logger,
    IServiceProvider serviceProvider,
    IOptions<DonDumbledoreConfig> options) : IHostedService
{
    private readonly DonDumbledoreConfig _config = options.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("starting bot");
        var token = _config.BotToken;
        if (token is null)
        {
            logger.LogWarning("bot token not found, bot will not launch");
            return;
        }

        if (client.ConnectionState != ConnectionState.Disconnected)
        {
            logger.LogWarning($"bot client is not in {nameof(ConnectionState.Disconnected)}, bot will not launch");
            return;
        }

        client.Connected += Client_Connected;
        client.SlashCommandExecuted += Client_SlashCommandExecuted;
        client.MessageReceived += Client_MessageReceived;
        client.Log += Client_Log;

        await client.LoginAsync(TokenType.Bot, token);
        logger.LogInformation("bot successfully logged in");
        await client.StartAsync();
        logger.LogInformation("bot successfully started");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (client.ConnectionState != ConnectionState.Connected)
        {
            logger.LogWarning($"bot client is not in {nameof(ConnectionState.Connected)}, bot will not stop");
            return;
        }

        await ForceStopAsync();
    }

    private Task Client_Log(LogMessage arg)
    {
        switch (arg.Severity)
        {
            case LogSeverity.Critical:
                logger.LogCritical(arg.Exception, "bot unhandled: {message}", arg.Message);
                break;
            case LogSeverity.Error:
                logger.LogError(arg.Exception, "bot unhandled: {message}", arg.Message);
                break;
            case LogSeverity.Warning:
                logger.LogWarning(arg.Exception, "bot unhandled: {message}", arg.Message);
                break;
            case LogSeverity.Info:
                logger.LogInformation(arg.Exception, "bot unhandled: {message}", arg.Message);
                break;
            case LogSeverity.Verbose:
                logger.LogInformation(arg.Exception, "bot unhandled: {message}", arg.Message);
                break;
            case LogSeverity.Debug:
                logger.LogDebug(arg.Exception, "bot unhandled: {message}", arg.Message);
                break;
            default:
                logger.LogInformation(arg.Exception, "bot unhandled: {message}", arg.Message);
                break;
        }

        return Task.CompletedTask;
    }

    private async Task Client_MessageReceived(SocketMessage arg)
    {
        logger.LogInformation("bot received a message");
        await mediator.Publish(new MessageReceivedNotification { SocketMessage = arg });
    }

    public async Task ForceStopAsync()
    {
        client.Connected -= Client_Connected;
        client.SlashCommandExecuted -= Client_SlashCommandExecuted;
        client.MessageReceived -= Client_MessageReceived;
        client.Log -= Client_Log;

        await client.LogoutAsync();
        logger.LogInformation("bot successfully logged out");
        await client.StopAsync();
        logger.LogInformation("bot successfully stopped");
    }

    private async Task Client_Connected()
    {
        await RegisterCommandsAsync();
    }

    private async Task RegisterCommandsAsync()
    {
        logger.LogInformation("isProduction: {isProd}, registerNewCommands: {regNew}", _config.IsProductionEnvironment, _config.RegisterNewCommands);

        if (_config.IsProductionEnvironment)
        {
            await ClearGuildCommandsAsync();

            if (_config.RegisterNewCommands)
            {
                await BulkOverwriteGlobalCommandsAsync();
            }
        }
        else
        {
            await ClearGlobalCommandsAsync();
            await BulkOverwriteGuildCommandsAsync();
        }

        return;

        async Task BulkOverwriteGuildCommandsAsync()
        {
            var services = serviceProvider.GetServices<IDonCommand>().ToList();
            var guilds = client.Guilds;
            await Parallel.ForEachAsync(guilds, async (guild, _) =>
            {
                var commandProperties = services.Select(x => x.CreateProperties()).ToArray();
                try
                {
                    await guild.BulkOverwriteApplicationCommandAsync(commandProperties);
                    logger.LogInformation("registered guild commands");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "error while bulk registering guild commands");
                }
            });
        }

        async Task BulkOverwriteGlobalCommandsAsync()
        {
            var services = serviceProvider.GetServices<IDonCommand>().ToList();
            var commandProperties = services.Select(x => x.CreateProperties()).ToArray();
            try
            {
                await client.BulkOverwriteGlobalApplicationCommandsAsync(commandProperties);
                logger.LogInformation("registered global commands");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "error while bulk registering global commands");
            }
        }

        async Task ClearGlobalCommandsAsync()
        {
            var commands = await client.GetGlobalApplicationCommandsAsync();

            foreach (var command in commands)
            {
                await command.DeleteAsync();
                logger.LogInformation("deleted global command: {command}", command.Name);
            }
        }

        async Task ClearGuildCommandsAsync()
        {
            var guilds = client.Guilds;

            await Parallel.ForEachAsync(guilds, async (guild, _) =>
            {
                await guild.DeleteApplicationCommandsAsync();
                logger.LogInformation("deleted guild commands, guild: {guildId}", guild.Id);
            });
        }
    }

    private async Task Client_SlashCommandExecuted(SocketSlashCommand arg)
    {
        logger.LogInformation("bot received slash command: {commandName}", arg.CommandName);

        var handler = serviceProvider.GetServices<IDonCommand>().SingleOrDefault(x => x.Name == arg.CommandName);
        if (handler is null)
        {
            logger.LogWarning("handler not found for command: {commandName}", arg.CommandName);
            return;
        }

        await handler.Handle(arg);
    }
}