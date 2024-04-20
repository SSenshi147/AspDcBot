using Discord;
using Discord.WebSocket;
using DonDumbledore.Logic.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DonDumbledore.Logic.SlashCommands;

public class UntrackCommand(IServiceProvider serviceProvider) : IDonCommand
{
    private const string NAME = "untrack";
    private const string DESCRIPTION = "A megadott üzenet trekkelését befejezi";
    private const string VALUE_OPTION = "untracking";
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
        var trackMessage = (string?)arg.Data.Options.FirstOrDefault()?.Value;
        if (string.IsNullOrEmpty(trackMessage))
        {
            await arg.RespondAsync("invalid input");
            return;
        }

        await using var scope = serviceProvider.CreateAsyncScope();
        await using var botDbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

        var model = await botDbContext.TrackedMessageModels.FirstOrDefaultAsync(x => x.MessageValue.Equals(trackMessage));
        if (model is null)
        {
            await arg.RespondAsync("Ezt eddig is leszartam, kisfiam.");
            return;
        }

        botDbContext.Remove(model);
        await botDbContext.SaveChangesAsync();

        await arg.RespondAsync($"Jólvan, Lőrinc fiú leszarja a \"{trackMessage}\" üzeneteket.");
    }
}