using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DonDumbledore.Logic;
using DonDumbledore.Logic.Data;
using DonDumbledore.Logic.Extensions;
using DonDumbledore.Logic.Services;
using DonDumbledore.Logic.SlashCommands;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace DonDumbledore.ConsoleHost;

internal class Program
{
    private static void Main(params string[] objects)
    {
        const string dbConnectionStringKey = "SqliteConnectionString";
        const string hangfireDbConnectionStringKey = "HangfireSqliteConnectionString";

        var builder = Host.CreateApplicationBuilder();

        var connectionString = builder.Configuration[dbConnectionStringKey];
        var hangfireConnectionString = builder.Configuration[hangfireDbConnectionStringKey];

        var registerNewCommands = false;
        if (objects is not null && objects.Length > 0)
        {
            registerNewCommands = bool.TryParse(objects[0], out var result) && result;
        }

        builder.Services.Configure<DonDumbledoreConfig>(builder.Configuration);
        builder.Services.PostConfigure<DonDumbledoreConfig>(config =>
        {
            config.IsProductionEnvironment = builder.Environment.IsProduction();
            config.RegisterNewCommands = registerNewCommands;
        });

        builder.Services.AddHangfire(config =>
        {
            config.UseSQLiteStorage(hangfireConnectionString);
        });
        builder.Services.AddHangfireServer();

        builder.Services.AddSingleton<DiscordSocketClient>();
        builder.Services.AddDbContext<BotDbContext>(opt => opt.UseSqlite(connectionString,
            x => x.MigrationsAssembly(Assembly.GetAssembly(typeof(BotDbContext))!.FullName)));
        builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<PingCommand>());
        builder.Services.AddSingleton<InteractionService>();
        builder.Services.AddSingleton<DiscordSocketClient>(sp => new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.All }));
        builder.Services.RegisterCommands();
        builder.Services.AddHostedService<DiscordBotService>();

        var host = builder.Build();

        using var scope = host.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<BotDbContext>();
        if (context.Database.GetPendingMigrations().Any())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            try
            {
                logger.LogInformation("trying to auto apply migrations");
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "error while auto applying migrations, quitting");
                return;
            }
        }

        host.Run();

        Console.WriteLine("Press ENTER to exit");
        Console.ReadLine();
    }
}