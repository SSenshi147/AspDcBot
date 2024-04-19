using Discord.WebSocket;
using Discord;
using DonDumbledore.Logic.Data;
using DonDumbledore.Logic.Requests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DonDumbledore.Logic.SlashCommands
{
    public class UntrackCommand(IServiceProvider serviceProvider) : IDonCommand
    {
        public string Name => NAME;

        private const string NAME = "untrack";
        private const string DESCRIPTION = "A megadott üzenet trekkelését befejezi";
        private const string VALUE_OPTION = "untracking";
        public SlashCommandProperties CreateProperties()
        {
            var builder = new SlashCommandBuilder();

            builder.WithName(NAME);
            builder.WithDescription(DESCRIPTION);
            builder.AddOption(VALUE_OPTION, ApplicationCommandOptionType.String, "üzenet", isRequired: true);
            return builder.Build();
        }

        public async Task Handle(SocketSlashCommand arg)
        {
            using var scope = serviceProvider.CreateAsyncScope();
            using var botDbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

            var trackMessage = (string?)arg.Data.Options.FirstOrDefault().Value;

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
}
