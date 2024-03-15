using System.Reflection;
using Discord;
using Discord.WebSocket;
using DonDumbledore.Logic.Attributes;
using DonDumbledore.Logic.Notifications;
using DonDumbledore.Logic.Requests;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DonDumbledore.Logic.Services;

public class DiscordBotService(
    DiscordSocketClient client,
    IMediator mediator,
    ILogger<DiscordBotService> logger,
    IConfiguration configuration) : IHostedService
{
    private const string TokenKey = "BotToken";

    private readonly IEnumerable<Type> _commands = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(RequestBase)));

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("starting bot");
        var token = configuration[TokenKey];
        if (token is null)
        {
            logger.LogWarning($"environment variable {TokenKey} not found, bot will not launch");
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

        await client.LoginAsync(TokenType.Bot, token);
        logger.LogInformation("bot successfully logged in");
        await client.StartAsync();
        logger.LogInformation("bot successfully started");
    }

    private async Task Client_MessageReceived(SocketMessage arg)
    {
        logger.LogInformation("bot received a message");
        await mediator.Publish(new MessageReceivedNotification
        {
            SocketMessage = arg
        });
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

    public async Task ForceStopAsync()
    {
        client.Connected -= Client_Connected;
        client.SlashCommandExecuted -= Client_SlashCommandExecuted;
        client.MessageReceived -= Client_MessageReceived;

        await client.LogoutAsync();
        logger.LogInformation("bot successfully logged out");
        await client.StopAsync();
        logger.LogInformation("bot successfully stopped");
    }

    private async Task Client_Connected()
    {
        // TODO: configból allowed servereket kiszedni
        // TODO: csak akkor regelni a commandot, ha nincs még regelve
        await ClearGlobalCommands();

        var guilds = client.Guilds;

        await Parallel.ForEachAsync(guilds, async (guild, _) =>
        {
            await guild.DeleteApplicationCommandsAsync();
            logger.LogInformation("deleted guild commands, guild: {guildId}", guild.Id);
            await RegisterManualCommands(guild);
        });
    }
    private async Task ClearGlobalCommands()
    {
        var commands = await client.GetGlobalApplicationCommandsAsync();

        foreach (var command in commands)
        {
            await command.DeleteAsync();
            logger.LogInformation("deleted global command: {command}", command.Name);
        }
    }

    private async Task RegisterManualCommands(SocketGuild guild)
    {
        foreach (var item in _commands)
        {
            var attribute = item.GetCustomAttribute<SlashCommandInfoAttribute>();
            if (attribute is null) continue;

            try
            {
                var command = new SlashCommandBuilder()
                        .WithName(attribute.Name)
                        .WithDescription(attribute.Description)
                        .Build();

                await guild.CreateApplicationCommandAsync(command);

                logger.LogInformation("registered guild command for guildId: {guildId}, command: {command}", guild.Id, command.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "miagecivan");
            }
        }
    }

    private async Task Client_SlashCommandExecuted(SocketSlashCommand arg)
    {
        logger.LogInformation("bot received slash command: {commandName}", arg.CommandName);

        var type = _commands.FirstOrDefault(x => x.GetCustomAttribute<SlashCommandInfoAttribute>()?.Name == arg.CommandName);
        if (type is null) return;

        var instance = Activator.CreateInstance(type, arg);
        if (instance is null) return;

        await mediator.Send(instance);
    }
}