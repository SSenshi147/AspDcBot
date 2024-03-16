using Discord;
using Discord.WebSocket;
using DonDumbledore.Logic.Requests;

namespace DonDumbledore.Logic.SlashCommands;

public class ReminderCommand : IDonCommand
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

    public async Task Handle(SocketSlashCommand command)
    {
        await command.RespondAsync(text: "sanyi");
    }

    //public async Task Handle(ReminderCommand request, CancellationToken cancellationToken)
//    {
//        var arg = request.Arg ?? throw new Exception();
        
//        var jobName = (string)arg.GetDataByName(request.Message)!;
//        var timing = (string)arg.GetDataByName(request.CronExpression)!;

//        try
//        {
//            if (await botDbContext.JobDataModels.AnyAsync(x => x.JobId == jobName, cancellationToken))
//            {
//                await arg.RespondAsync(text: "job already exists");
//                return;
//            }

//            await botDbContext.JobDataModels.AddAsync(new JobData
//            {
//                JobId = jobName,
//                UserId = arg.User.Id,

//            }, cancellationToken);
//            await botDbContext.SaveChangesAsync(cancellationToken);

//            RecurringJob.AddOrUpdate(jobName, () => PingTask(jobName), timing);
//            await arg.RespondAsync(text: "job added");
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "error while adding recurring job test-pinger");
//        }
//    }

//    public async Task PingTask(string jobName)
//    {
//        try
//        {
//            var job = await botDbContext.JobDataModels.FirstOrDefaultAsync(x => x.JobId == jobName);
//            var user = await discordSocketClient.GetUserAsync(job.UserId);

//            await user.SendMessageAsync(text: $"ping {jobName}");
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "error while executing recurring job test-pinger");
//        }
//    }
}

//public class ReminderCommandHandler(
//    BotDbContext botDbContext,
//    ILogger<ReminderCommandHandler> logger,
//    DiscordSocketClient discordSocketClient) : IRequestHandler<ReminderCommand>
//{
//    public async Task Handle(ReminderCommand request, CancellationToken cancellationToken)
//    {
//        var arg = request.Arg ?? throw new Exception();
        
//        var jobName = (string)arg.GetDataByName(request.Message)!;
//        var timing = (string)arg.GetDataByName(request.CronExpression)!;

//        try
//        {
//            if (await botDbContext.JobDataModels.AnyAsync(x => x.JobId == jobName, cancellationToken))
//            {
//                await arg.RespondAsync(text: "job already exists");
//                return;
//            }

//            await botDbContext.JobDataModels.AddAsync(new JobData
//            {
//                JobId = jobName,
//                UserId = arg.User.Id,

//            }, cancellationToken);
//            await botDbContext.SaveChangesAsync(cancellationToken);

//            RecurringJob.AddOrUpdate(jobName, () => PingTask(jobName), timing);
//            await arg.RespondAsync(text: "job added");
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "error while adding recurring job test-pinger");
//        }
//    }

//    public async Task PingTask(string jobName)
//    {
//        try
//        {
//            var job = await botDbContext.JobDataModels.FirstOrDefaultAsync(x => x.JobId == jobName);
//            var user = await discordSocketClient.GetUserAsync(job.UserId);

//            await user.SendMessageAsync(text: $"ping {jobName}");
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "error while executing recurring job test-pinger");
//        }
//    }
//}

//public async Task Handle(PingQueueRemoveRequest request, CancellationToken cancellationToken)
//    {
//        var arg = request.Arg;
//        var asd = request.Arg.Data.Options.SingleOrDefault(x => x.Name == "message") ?? throw new Exception();
//    var jobId = (string)asd?.Value ?? throw new Exception();
//    try
//    {
//            var toRemove = await botDbContext.JobDataModels.FirstOrDefaultAsync(x => x.JobId == jobId, cancellationToken);

//            if (toRemove is null || toRemove == default)
//            {
//                await arg.RespondAsync(text: "job doesnt exist");
//            return;
//        }
//        botDbContext.JobDataModels.Remove(toRemove);
//            await botDbContext.SaveChangesAsync(cancellationToken);

//            RecurringJob.RemoveIfExists(jobId);

//            await arg.RespondAsync(text: "job removed");
//    }
//    catch (Exception ex)
//    {
//            logger.LogError(ex, "error while removing recurring job test-pinger");
//        }
//    }