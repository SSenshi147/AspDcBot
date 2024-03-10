using AspDcBot.Commands;
using AspDcBot.Models;
using Discord;
using Discord.WebSocket;
using MediatR;
using System.Reflection;

namespace AspDcBot.Services;

public class DiscordBotService(
    DiscordSocketClient client,
    IMediator mediator,
    ILogger<DiscordBotService> logger,
    IConfiguration configuration)
{
    // TODO: configba
    private const string TokenKey = "BotToken";
    private const ulong DevServerId = 1046516338119675955;

    private readonly IEnumerable<Type> _commands = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(Requests)));

    public async Task StartAsync()
    {
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
            SocketMessage = arg,
        });
    }

    public async Task StopAsync()
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

    public async Task PingAsync(CancellationToken cancellationToken, string message = "ping")
    {
        await Parallel.ForEachAsync(client.Guilds, cancellationToken, async (guild, token) =>
        {
            await Parallel.ForEachAsync(guild.Channels, cancellationToken, async (channel, token) =>
            {
                if (channel is not null && channel is ITextChannel textChannel)
                {
                    await textChannel.SendMessageAsync(text: message);
                }
            });
        });
    }

    public List<Guild> GetGuildsAndChannels()
    {
        var response = new List<Guild>();

        foreach (var guild in client.Guilds)
        {
            var guildToAdd = new Guild
            {
                Id = guild.Id.ToString(),
                Name = guild.Name,
            };

            foreach (var channel in guild.Channels)
            {
                var channelToAdd = new Channel
                {
                    Id = channel.Id.ToString(),
                    Name = channel.Name,
                    Type = channel.GetType().Name.ToString(),
                };

                guildToAdd.Channels.Add(channelToAdd);
            }

            response.Add(guildToAdd);
        }

        return response;
    }

    public async Task ClearGlobalCommands()
    {
        var commands = await client.GetGlobalApplicationCommandsAsync();

        foreach (var command in commands)
        {
            await command.DeleteAsync();
        }
    }

    private async Task Client_Connected()
    {
        // TODO: configból allowed servereket kiszedni
        // TODO: csak akkor regelni a commandot, ha nincs még regelve
        //var devGuild = client.Guilds.SingleOrDefault(x => x.Id == DevServerId);

        //if (devGuild is null) return;

        var guilds = client.Guilds;

        await Parallel.ForEachAsync(guilds, async (guild, _) =>
        {
            await guild.DeleteApplicationCommandsAsync();
            await RegisterManualCommands(guild);
        });

        //await devGuild.DeleteApplicationCommandsAsync();

        //await RegisterManualCommands(devGuild);
    }

    private async Task RegisterManualCommands(SocketGuild guild)
    {
        foreach (var item in _commands)
        {
            var attribute = item.GetCustomAttribute<SlashCommandInfoAttribute>();
            if (attribute is null) continue;

            var command = new SlashCommandBuilder()
               .WithName(attribute.Name)
               .WithDescription(attribute.Description)
               .Build();

            await guild.CreateApplicationCommandAsync(command);
        }
    }

    private async Task Client_SlashCommandExecuted(SocketSlashCommand arg)
    {
        logger.LogInformation($"bot received slash command: {arg.CommandName}");

        var type = _commands.FirstOrDefault(x => x.GetCustomAttribute<SlashCommandInfoAttribute>()?.Name == arg.CommandName);
        if (type is null) return;

        var instance = Activator.CreateInstance(type, arg);
        if (instance is null) return;

        await mediator.Send(instance);
    }
}
