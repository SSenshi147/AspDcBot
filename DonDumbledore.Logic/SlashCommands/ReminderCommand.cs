using Cronos;
using Discord;
using Discord.WebSocket;
using DonDumbledore.Logic.Data;
using DonDumbledore.Logic.Requests;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DonDumbledore.Logic.SlashCommands;

public class ReminderCommand(
    DiscordSocketClient discordSocketClient,
    ILogger<ReminderCommand> logger,
    IServiceProvider serviceProvider) : IDonCommand
{
    public string Name => NAME;

    private const string NAME = "reminder";
    private const string DESCRIPTION = "deals with reminders";
    private const string OPTION_ADD = "add";
    private const string OPTION_ADD_MESSAGE = "message";
    private const string OPTION_ADD_CRON = "cron";
    private const string OPTION_ADD_REMINDERCRON = "remindercron";
    private const string OPTION_REMOVE = "remove";
    private const string OPTION_REMOVE_MESSAGE = "message";
    private const string OPTION_ACK = "ack";
    private const string OPTION_ACK_MESSAGE = "message";

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
                    .WithRequired(true))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(OPTION_ADD_REMINDERCRON)
                    .WithDescription("the cronexpression of the reminder job")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(false)))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(OPTION_REMOVE)
                .WithDescription("remove an existing reminder")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(OPTION_REMOVE_MESSAGE)
                    .WithDescription("the id of the job")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(true)))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(OPTION_ACK)
                .WithDescription("acknowledges job, stops reminding")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(OPTION_ACK_MESSAGE)
                    .WithDescription("the id of the job")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(true)));

        return builer.Build();
    }

    public async Task Handle(SocketSlashCommand arg)
    {
        try
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
                case OPTION_ACK:
                    await HandleAck(subcommand, arg);
                    break;
                default: break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "miafaszvan");
        }
    }

    private async Task HandleAdd(SocketSlashCommandDataOption option, SocketSlashCommand arg)
    {
        var jobName = (string)option.Options.SingleOrDefault(x => x.Name == OPTION_ADD_MESSAGE).Value;
        var timing = (string)option.Options.SingleOrDefault(x => x.Name == OPTION_ADD_CRON).Value;
        var reminderTiming = (string?)option.Options.SingleOrDefault(x => x.Name == OPTION_ADD_REMINDERCRON)?.Value;

        using var scope = serviceProvider.CreateAsyncScope();
        using var botDbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

        try
        {
            if (await botDbContext.JobDataModels.AnyAsync(x => x.JobId == jobName))
            {
                await arg.FollowupAsync(text: "job already exists");
                return;
            }

            if (!CronExpression.TryParse(timing, out _))
            {
                await arg.FollowupAsync(text: "invalid cron");
                return;
            }

            if (reminderTiming?.Length > 0 && !CronExpression.TryParse(reminderTiming, out _))
            {
                await arg.FollowupAsync(text: "invalid cron");
                return;
            }

            await botDbContext.JobDataModels.AddAsync(new JobData
            {
                JobId = jobName,
                UserId = arg.User.Id,
            });
            await botDbContext.SaveChangesAsync();
            RecurringJob.AddOrUpdate(jobName, () => PingTask(jobName, reminderTiming), timing);

            await arg.FollowupAsync(text: "job added");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error while adding recurring job test-pinger");
        }
    }

    public async Task PingTask(string jobName, string? recurringCron = null)
    {
        try
        {
            using var scope = serviceProvider.CreateAsyncScope();
            using var botDbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

            var job = await botDbContext.JobDataModels.FirstOrDefaultAsync(x => x.JobId == jobName);
            var user = await discordSocketClient.GetUserAsync(job.UserId);

            var message = await user.SendMessageAsync(text: $"ping {jobName}");

            if (recurringCron?.Length > 0 && job.ReminderJobId is null)
            {
                var reminderJobId = $"{job.JobId}-reminder";
                job.ReminderJobId = reminderJobId;

                try
                {
                    RecurringJob.AddOrUpdate(reminderJobId, () => PingTaskReminder(jobName), recurringCron);
                    botDbContext.JobDataModels.Update(job);
                    await botDbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "ilyen nincs bazmeg");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error while executing recurring job test-pinger");
        }
    }

    public void PingTaskReminder(string originalJobName)
    {
        try
        {
            RecurringJob.TriggerJob(originalJobName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error while executing recurring job reminder");
        }
    }

    private async Task HandleRemove(SocketSlashCommandDataOption option, SocketSlashCommand arg)
    {
        var jobId = (string)option.Options.SingleOrDefault(x => x.Name == OPTION_REMOVE_MESSAGE).Value;

        try
        {
            using var scope = serviceProvider.CreateAsyncScope();
            using var botDbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

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

    private async Task HandleAck(SocketSlashCommandDataOption option, SocketSlashCommand arg)
    {
        var jobId = (string)option.Options.SingleOrDefault(x => x.Name == OPTION_ACK_MESSAGE).Value;

        try
        {
            using var scope = serviceProvider.CreateAsyncScope();
            using var botDbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

            var toRemove = await botDbContext.JobDataModels.FirstOrDefaultAsync(x => x.JobId == jobId);

            if (toRemove is null || toRemove == default)
            {
                await arg.FollowupAsync(text: "job doesnt exist");
                return;
            }

            RecurringJob.RemoveIfExists(toRemove.ReminderJobId);
            toRemove.ReminderJobId = null;
            botDbContext.JobDataModels.Update(toRemove);
            await botDbContext.SaveChangesAsync();

            await arg.FollowupAsync(text: "acknowledged");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error while removing recurring job test-pinger");
        }
    }
}