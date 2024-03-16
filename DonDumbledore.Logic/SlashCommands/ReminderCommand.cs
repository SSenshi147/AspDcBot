using Discord;
using Discord.WebSocket;
using DonDumbledore.Logic.Data;
using DonDumbledore.Logic.Requests;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DonDumbledore.Logic.SlashCommands;

public class ReminderCommand(
    BotDbContext botDbContext,
    DiscordSocketClient discordSocketClient,
    ILogger<ReminderCommand> logger) : IDonCommand
{
    public string Name => NAME;

    private const string NAME = "reminder";
    private const string DESCRIPTION = "deals with reminders";
    private const string OPTION_ADD = "add";
    private const string OPTION_ADD_MESSAGE = "message";
    private const string OPTION_ADD_CRON = "cron";
    private const string OPTION_REMOVE = "remove";
    private const string OPTION_REMOVE_MESSAGE = "message";

    public SlashCommandProperties CreateProperties()
    {
        var builer = new SlashCommandBuilder()
            .WithName(NAME)
            .WithDescription(DESCRIPTION)
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(OPTION_ADD)
                .WithDescription("add a new reminder")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(OPTION_ADD_MESSAGE)
                    .WithDescription("the id of the job and the message to send")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(true))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(OPTION_ADD_CRON)
                    .WithDescription("the cronexpression of the job")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(true)))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(OPTION_REMOVE)
                .WithDescription("remove an existing reminder")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(OPTION_REMOVE_MESSAGE)
                    .WithDescription("the id of the job")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(true)));

        return builer.Build();
    }

    public async Task Handle(SocketSlashCommand arg)
    {
        await arg.RespondAsync(text: "received");
        var subcommand = arg.Data.Options.SingleOrDefault();

        switch (subcommand.Name)
        {
            case OPTION_ADD:
                await HandleAdd(subcommand, arg);
                break;
            case OPTION_REMOVE:
                await HandleRemove(subcommand, arg);
                break;
            default: break;
        }
    }

    private async Task HandleAdd(SocketSlashCommandDataOption option, SocketSlashCommand arg)
    {
        var jobName = (string)option.Options.SingleOrDefault(x => x.Name == OPTION_ADD_MESSAGE).Value;
        var timing = (string)option.Options.SingleOrDefault(x => x.Name == OPTION_ADD_CRON).Value;

        try
        {
            if (await botDbContext.JobDataModels.AnyAsync(x => x.JobId == jobName))
            {
                await arg.FollowupAsync(text: "job already exists");
                return;
            }

            await botDbContext.JobDataModels.AddAsync(new JobData
            {
                JobId = jobName,
                UserId = arg.User.Id,

            });
            await botDbContext.SaveChangesAsync();

            RecurringJob.AddOrUpdate(jobName, () => PingTask(jobName), timing);
            await arg.FollowupAsync(text: "job added");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error while adding recurring job test-pinger");
        }
    }

    public async Task PingTask(string jobName)
    {
        try
        {
            var job = await botDbContext.JobDataModels.FirstOrDefaultAsync(x => x.JobId == jobName);
            var user = await discordSocketClient.GetUserAsync(job.UserId);

            await user.SendMessageAsync(text: $"ping {jobName}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error while executing recurring job test-pinger");
        }
    }

    private async Task HandleRemove(SocketSlashCommandDataOption option, SocketSlashCommand arg)
    {
        var jobId = (string)option.Options.SingleOrDefault(x => x.Name == OPTION_REMOVE_MESSAGE).Value;
        
        try
        {
            var toRemove = await botDbContext.JobDataModels.FirstOrDefaultAsync(x => x.JobId == jobId);

            if (toRemove is null || toRemove == default)
            {
                await arg.FollowupAsync(text: "job doesnt exist");
                return;
            }
            botDbContext.JobDataModels.Remove(toRemove);
            await botDbContext.SaveChangesAsync();

            RecurringJob.RemoveIfExists(jobId);

            await arg.FollowupAsync(text: "job removed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error while removing recurring job test-pinger");
        }
    }
}