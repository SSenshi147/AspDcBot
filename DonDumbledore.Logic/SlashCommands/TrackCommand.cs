using Discord;
using Discord.WebSocket;
using DonDumbledore.Logic.Data;
using DonDumbledore.Logic.Notifications;
using DonDumbledore.Logic.Requests;
using Hangfire.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualBasic.CompilerServices;

namespace DonDumbledore.Logic.SlashCommands
{
    public class TrackCommand(IServiceProvider serviceProvider) : IDonCommand
    {
        public string Name => NAME;

        private const string NAME = "track";
        private const string DESCRIPTION = "A megadott üzenetet, akár emodzsit trekkeli";
        private const string VALUE_OPTION = "tracking";
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

            if (model is not null)
            {

                await arg.RespondAsync("Golyót akarsz? Ezt már figyelem.");
            }

            model = new TrackedMessage()
            {
                MessageValue = trackMessage,
            };

            await botDbContext.AddAsync(model);
            await botDbContext.SaveChangesAsync();

            await arg.RespondAsync($"Jólvan, a Lőrinc fiú számolgatja a \"{trackMessage}\" üzeneteket.");
        }
    }
}
