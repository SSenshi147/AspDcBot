using Discord;
using Discord.WebSocket;
using DonDumbledore.Logic.Attributes;
using DonDumbledore.Logic.Data;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DonDumbledore.Logic.Requests;

[SlashCommandInfo("ping-queue", "ping-queue", Option = "message")]
public class PingQueueRequest(SocketSlashCommand myProperty) : RequestBase(myProperty)
{
}

[SlashCommandInfo("ping-queue-remove", "ping-queue-remove", Option = "message")]
public class PingQueueRemoveRequest(SocketSlashCommand myProperty) : RequestBase(myProperty)
{
}

public class PingQueueRequestHandler(
    ILogger<PingQueueRequestHandler> logger,
    BotDbContext botDbContext,
    DiscordSocketClient discordSocketClient) : IRequestHandler<PingQueueRequest>
{
    private const string atEveryMinute = "*/1 * * * *";

    public async Task Handle(PingQueueRequest request, CancellationToken cancellationToken)
    {
        var arg = request.Arg;
        var asd = request.Arg.Data.Options.SingleOrDefault(x => x.Name == "message") ?? throw new Exception();
        var jobId = (string)asd?.Value ?? throw new Exception();


        try
        {
            if (await botDbContext.JobDataModels.AnyAsync(x => x.JobId == jobId, cancellationToken))
            {
                await arg.RespondAsync(text: "job already exists");
                return;
            }

            await botDbContext.JobDataModels.AddAsync(new JobData
            {
                JobId = jobId,
                UserId = arg.User.Id,

            }, cancellationToken);
            await botDbContext.SaveChangesAsync(cancellationToken);

            RecurringJob.AddOrUpdate(jobId, () => PingTask(jobId), atEveryMinute);
            //RecurringJob.AddOrUpdate(jobId, () => BackgroundJobHolder.PingTask(arg), atEveryMinute);
            await arg.RespondAsync(text: "job added");
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
}

public class PingQueueRemoveRequestHandler(
    ILogger<PingQueueRemoveRequestHandler> logger,
    BotDbContext botDbContext) : IRequestHandler<PingQueueRemoveRequest>
{
    public async Task Handle(PingQueueRemoveRequest request, CancellationToken cancellationToken)
    {
        var arg = request.Arg;
        var asd = request.Arg.Data.Options.SingleOrDefault(x => x.Name == "message") ?? throw new Exception();
        var jobId = (string)asd?.Value ?? throw new Exception();

        try
        {
            var toRemove = await botDbContext.JobDataModels.FirstOrDefaultAsync(x => x.JobId == jobId, cancellationToken);

            if (toRemove is null || toRemove == default)
            {
                await arg.RespondAsync(text: "job doesnt exist");
                return;
            }

            botDbContext.JobDataModels.Remove(toRemove);
            await botDbContext.SaveChangesAsync(cancellationToken);

            RecurringJob.RemoveIfExists(jobId);

            await arg.RespondAsync(text: "job removed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error while removing recurring job test-pinger");
        }
    }
}