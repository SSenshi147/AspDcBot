using AspDcBot.Data;
using AspDcBot.Services;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsoleHost;

internal class Program
{
    static void Main(string[] args)
    {
        const string dbConnectionStringKey = "SqliteConnectionString";

        var builder = Host.CreateApplicationBuilder();

        var connectionString = builder.Configuration[dbConnectionStringKey];

        var envName = builder.Environment.ApplicationName;
        var isDev = builder.Environment.IsDevelopment(); // DOTNET_ENVIRONMENT = Development variable

        builder.Services.AddSingleton<DiscordSocketClient>();
        builder.Services.AddDbContext<BotDbContext>(opt => opt.UseSqlite(connectionString), ServiceLifetime.Singleton);
        builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<AspDcBot.Program>());
        builder.Services.AddSingleton<InteractionService>();
        builder.Services.AddSingleton<DiscordSocketClient>(sp => new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = Discord.GatewayIntents.All
        }));
        builder.Services.AddSingleton<DiscordBotService>();
        builder.Services.AddHostedService<Test>();

        builder.Build().Run();
    }
}

public class Test(DiscordBotService discordBotService) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Console.Out.WriteLineAsync("starting");

        await discordBotService.StartAsync();

        await Console.Out.WriteLineAsync("started");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Console.Out.WriteLineAsync("stopping");

        await discordBotService.StopAsync();

        await Console.Out.WriteLineAsync("stopped");
    }
}