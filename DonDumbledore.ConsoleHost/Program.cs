using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DonDumbledore.Logic.Data;
using DonDumbledore.Logic.Requests;
using DonDumbledore.Logic.Services;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace DonDumbledore.ConsoleHost;

internal class Program
{
    private static void Main()
    {
        const string dbConnectionStringKey = "SqliteConnectionString";
        const string hangfireDbConnectionStringKey = "HangfireSqliteConnectionString";

        var builder = Host.CreateApplicationBuilder();

        var connectionString = builder.Configuration[dbConnectionStringKey];
        var hangfireConnectionString = builder.Configuration[hangfireDbConnectionStringKey];

        var envName = builder.Environment.ApplicationName;
        var isDev = builder.Environment.IsDevelopment(); // DOTNET_ENVIRONMENT = Development variable

        builder.Services.AddHangfire(config =>
        {
            config.UseSQLiteStorage(hangfireConnectionString);
        });
        builder.Services.AddHangfireServer();

        builder.Services.AddSingleton<DiscordSocketClient>();
        builder.Services.AddDbContext<BotDbContext>(opt => opt.UseSqlite(connectionString,
            x => x.MigrationsAssembly(Assembly.GetAssembly(typeof(BotDbContext))!.FullName)), ServiceLifetime.Singleton);
        builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<RequestBase>());
        builder.Services.AddSingleton<InteractionService>();
        builder.Services.AddSingleton<DiscordSocketClient>(sp => new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All
        }));
        builder.Services.AddHostedService<DiscordBotService>();

        builder.Build().Run();

        Console.WriteLine("Press ENTER to exit");
        Console.ReadLine();
    }
}