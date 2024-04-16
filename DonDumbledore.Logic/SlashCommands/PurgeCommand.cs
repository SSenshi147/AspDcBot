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
    public class PurgeCommand(IServiceProvider serviceProvider) : IDonCommand
    {
        public string Name => NAME;

        private const string NAME = "purge";
        private const string DESCRIPTION = "Golyót kap minden ilyen üzenet";
        private const string VALUE_OPTION = "purge";
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

            var purgeMessage = (string?)arg.Data.Options.FirstOrDefault().Value;

            var model =  botDbContext.MessageModels.Where(x => x.MessageValue.Equals(purgeMessage)).ToList();

            if (model.Count == 0)
            {

                await arg.RespondAsync("Mit akarsz kitörölni rózsabogaram?");
            }

            botDbContext.MessageModels.RemoveRange(model);
            await botDbContext.SaveChangesAsync();

            await arg.RespondAsync($"Jólvan, kiküldtem az egységet, hogy golyót adjon a \"{purgeMessage}\" üzeneteknek.");
        }
    }
}
