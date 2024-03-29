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
    private const string OPTION_ADD_ID = "id";
    private const string OPTION_ADD_CRON = "cron";
    private const string OPTION_ADD_REMINDERCRON = "remindercron";
    private const string OPTION_ADD_MESSAGE = "message";
    private const string OPTION_REMOVE = "remove";
    private const string OPTION_REMOVE_ID = "id";
    private const string OPTION_ACK = "ack";
    private const string OPTION_ACK_ID = "id";
    private const string OPTION_ONETIME = "onetime";
    private const string OPTION_ONETIME_DATETIME = "datetime";
    private const string OPTION_ONETIME_MESSAGE = "message";

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
                    .WithName(OPTION_ADD_ID)
                    .WithDescription("the id of the job and the message to send")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(true))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(OPTION_ADD_CRON)
                    .WithDescription("the cronexpression of the job")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(true))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(OPTION_ADD_MESSAGE)
                    .WithDescription("the message to send, if not set, the id will be sent")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(false))
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
                    .WithName(OPTION_REMOVE_ID)
                    .WithDescription("the id of the job")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(true)))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(OPTION_ACK)
                .WithDescription("acknowledges job, stops reminding")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(OPTION_ACK_ID)
                    .WithDescription("the id of the job")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(true)))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(OPTION_ONETIME)
                .WithDescription("adds a one-time reminder")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(OPTION_ONETIME_DATETIME, ApplicationCommandOptionType.String, "the ISO datetime of the reminer", isRequired: true)
                .AddOption(OPTION_ONETIME_MESSAGE, ApplicationCommandOptionType.String, "the message to send", isRequired: true));

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
                case OPTION_ONETIME:
                    await HandleOneTime(subcommand, arg);
                    break;
                default: break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "miafaszvan");
        }
    }

    private async Task HandleOneTime(SocketSlashCommandDataOption option, SocketSlashCommand arg)
    {
        var timing = (string)option.Options.SingleOrDefault(x => x.Name == OPTION_ONETIME_DATETIME).Value;
        var message = (string)option.Options.SingleOrDefault(x => x.Name == OPTION_ONETIME_MESSAGE).Value;

        if (!DateTime.TryParse(timing, out var dateTime))
        {
            await arg.FollowupAsync(text: "invalid timing, provide valid ISO format");
            return;
        }

        try
        {
            BackgroundJob.Schedule(() => OneTimeReminder(arg.Channel.Id, message), dateTime);
            await arg.FollowupAsync(text: $"added for {dateTime}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error while adding one-time reminder");
        }
    }

    public async Task OneTimeReminder(ulong channelId, string message)
    {
        var channel = await discordSocketClient.GetChannelAsync(channelId);
        if (channel is not IMessageChannel messageChannel)
        {
            logger.LogWarning("channel {channelId} is not IMessageChannel", channel.Id);
            return;
        }

        await messageChannel.SendMessageAsync(text: message);
    }

    private async Task HandleAdd(SocketSlashCommandDataOption option, SocketSlashCommand arg)
    {
        var jobName = (string)option.Options.SingleOrDefault(x => x.Name == OPTION_ADD_ID).Value;
        var timing = (string)option.Options.SingleOrDefault(x => x.Name == OPTION_ADD_CRON).Value;
        var reminderTiming = (string?)option.Options.SingleOrDefault(x => x.Name == OPTION_ADD_REMINDERCRON)?.Value;
        var message = (string?)option.Options.SingleOrDefault(x => x.Name == OPTION_ADD_MESSAGE)?.Value;

        using var scope = serviceProvider.CreateAsyncScope();
        using var botDbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

        try
        {
            var channelId = arg.Channel.Id;

            if (await botDbContext.JobDataModels.AnyAsync(x => x.JobId == jobName && x.ChannelId == arg.Channel.Id))
            {
                await arg.FollowupAsync(text: "job already exists");
                return;
            }

            if (!CronExpression.TryParse(timing, out _))
            {
                await arg.FollowupAsync(text: $"invalid cron: {nameof(timing)}");
                return;
            }

            if (reminderTiming?.Length > 0 && !CronExpression.TryParse(reminderTiming, out _))
            {
                await arg.FollowupAsync(text: $"invalid cron: {nameof(reminderTiming)}");
                return;
            }

            var newJob = (await botDbContext.JobDataModels.AddAsync(new JobData
            {
                JobId = jobName,
                ChannelId = channelId,
                Message = message,
            })).Entity;
            await botDbContext.SaveChangesAsync();
            RecurringJob.AddOrUpdate(newJob.HangfireJobId, () => PingTask(newJob, reminderTiming), timing, new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            });

            await arg.FollowupAsync(text: "job added");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error while adding recurring job test-pinger");
        }
    }

    public async Task PingTask(JobData jobData, string? recurringCron = null)
    {
        try
        {
            using var scope = serviceProvider.CreateAsyncScope();
            using var botDbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

            var job = await botDbContext.JobDataModels.FirstOrDefaultAsync(x => x.JobId == jobData.JobId && x.ChannelId == jobData.ChannelId);
            var channel = await discordSocketClient.GetChannelAsync(job.ChannelId);

            if (channel is not IMessageChannel messageChannel)
            {
                logger.LogWarning("channel {channelId} is not IMessageChannel", channel.Id);
                return;
            }

            var message = await messageChannel.SendMessageAsync(text: $"[{jobData.JobId}]: {jobData.Message ?? jobData.JobId}");

            if (recurringCron?.Length > 0 && job.ReminderJobId is null)
            {
                job.ReminderJobId = job.HangfireReminderJobId;

                try
                {
                    RecurringJob.AddOrUpdate(job.HangfireReminderJobId, () => PingTaskReminder(job.HangfireJobId), recurringCron, new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.Local
                    });
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
        var jobId = (string)option.Options.SingleOrDefault(x => x.Name == OPTION_REMOVE_ID).Value;

        try
        {
            using var scope = serviceProvider.CreateAsyncScope();
            using var botDbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

            var toRemove = await botDbContext.JobDataModels.FirstOrDefaultAsync(x => x.JobId == jobId && x.ChannelId == arg.Channel.Id);

            if (toRemove is null || toRemove == default)
            {
                await arg.FollowupAsync(text: "job doesnt exist");
                return;
            }

            RecurringJob.RemoveIfExists(toRemove.HangfireJobId);

            botDbContext.JobDataModels.Remove(toRemove);
            await botDbContext.SaveChangesAsync();

            await arg.FollowupAsync(text: "job removed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error while removing recurring job test-pinger");
        }
    }

    private async Task HandleAck(SocketSlashCommandDataOption option, SocketSlashCommand arg)
    {
        var jobId = (string)option.Options.SingleOrDefault(x => x.Name == OPTION_ACK_ID).Value;

        try
        {
            using var scope = serviceProvider.CreateAsyncScope();
            using var botDbContext = scope.ServiceProvider.GetRequiredService<BotDbContext>();

            var toRemove = await botDbContext.JobDataModels.FirstOrDefaultAsync(x => x.JobId == jobId && x.ChannelId == arg.Channel.Id);

            if (toRemove is null || toRemove == default)
            {
                await arg.FollowupAsync(text: "job doesnt exist");
                return;
            }

            RecurringJob.RemoveIfExists(toRemove.HangfireReminderJobId);
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