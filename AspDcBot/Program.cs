using AspDcBot.Data;
using AspDcBot.Services;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace AspDcBot;

public class Program
{
    public static void Main(string[] args)
    {
        const string sqliteConnectionString = "Data Source=/home/app/coffee.sqlite";

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddDbContext<BotDbContext>(opt => opt.UseSqlite(sqliteConnectionString));
        builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<Program>());
        builder.Services.AddSingleton<InteractionService>();
        builder.Services.AddSingleton<DiscordSocketClient>(sp => new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = Discord.GatewayIntents.All
        }));
        builder.Services.AddSingleton<DiscordBotService>();

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
