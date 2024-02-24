using AspDcBot.Commands;
using AspDcBot.Models;
using Discord;
using Discord.WebSocket;
using MediatR;
using System.Reflection;

namespace AspDcBot.Services;

public class DiscordBotService(
    DiscordSocketClient client,
    IMediator mediator)
{
    // TODO: configba
    private const string TOKEN = "MTA0Njc0MDkzNzk5ODU5ODE1NA.GO8vxV.P9S_ZcbGd0u4UXwxmCaN1yz2z3c-Mn9RXlYemI";
    private const ulong DEV_SERVER_ID = 1046516338119675955;

    private readonly IEnumerable<Type> commands = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(Requests)));

    public async Task StartAsync()
    {
        if (client.ConnectionState != ConnectionState.Disconnected)
        {
            return;
        }

        client.Connected += Client_Connected;
        client.SlashCommandExecuted += Client_SlashCommandExecuted;

        await client.LoginAsync(TokenType.Bot, TOKEN);
        await client.StartAsync();
    }

    public async Task StopAsync()
    {
        if (client.ConnectionState != ConnectionState.Connected)
        {
            return;
        }

        await ForceStopAsync();
    }

    public async Task ForceStopAsync()
    {
        client.Connected -= Client_Connected;
        client.SlashCommandExecuted -= Client_SlashCommandExecuted;

        await client.LogoutAsync();
        await client.StopAsync();
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
        var devGuild = client.Guilds.SingleOrDefault(x => x.Id == DEV_SERVER_ID);

        if (devGuild is null) return;

        await devGuild.DeleteApplicationCommandsAsync();

        await RegisterManualCommands(devGuild);
    }

    private async Task RegisterManualCommands(SocketGuild guild)
    {
        foreach (var item in commands)
        {
            var attribute = item.GetCustomAttribute<SlashCommandInfoAttribute>();
            if (attribute is null) return;

            var command = new SlashCommandBuilder()
               .WithName(attribute.Name)
               .WithDescription(attribute.Description)
               .Build();

            await guild.CreateApplicationCommandAsync(command);
        }
    }

    private async Task Client_SlashCommandExecuted(SocketSlashCommand arg)
    {
        var type = commands.FirstOrDefault(x => x.GetCustomAttribute<SlashCommandInfoAttribute>()?.Name == arg.CommandName);
        if (type is null) return;

        var instance = Activator.CreateInstance(type, arg);
        if (instance is null) return;

        await mediator.Send(instance);
    }
}
