using Discord;
using Discord.WebSocket;
using DonDumbledore.Logic.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DonDumbledore.Logic.SlashCommands;

public class PurgeCommand(IServiceProvider serviceProvider) : IDonCommand
{
    private const string NAME = "purge";
    private const string DESCRIPTION = "Golyót kap minden ilyen üzenet";
    private const string VALUE_OPTION = "purge";
    public string Name => NAME;

    public SlashCommandProperties CreateProperties()
    {
        var builder = new SlashCommandBuilder();

        builder.WithName(NAME);
        builder.WithDescription(DESCRIPTION);
        builder.AddOption(VALUE_OPTION, ApplicationCommandOptionType.String, "üzenet", true);
        return builder.Build();
    }

    public async Task Handle(SocketSlashCommand arg)
    {
        var purgeMessage = (string?)arg.Data.Options.FirstOrDefault()?.Value;
        if (string.IsNullOrEmpty(purgeMessage))
        {
            await arg.RespondAsync("Mit akarsz kitörölni rózsabogaram?");
            return;
        }
        
        await using var scope = serviceProvider.CreateAsyncScope();
        await using var botDbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

        var model = await botDbContext.MessageModels.Where(x => x.MessageValue.Equals(purgeMessage)).ToListAsync();
        if (model.Count == 0)
        {
            await arg.RespondAsync("Mit akarsz kitörölni rózsabogaram?");
            return;
        }

        botDbContext.MessageModels.RemoveRange(model);
        await botDbContext.SaveChangesAsync();

        await arg.RespondAsync($"Jólvan, kiküldtem az egységet, hogy golyót adjon a \"{purgeMessage}\" üzeneteknek.");
    }
}