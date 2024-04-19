using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DonDumbledore.Logic.Data;

public class BotDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<DrinkModel> DrinkModels
    {
        get;
        set;
    }

    public DbSet<UserData> UserDataModels
    {
        get;
        set;
    }

    public DbSet<JobData> JobDataModels
    {
        get;
        set;
    }
}

public class DrinkModel
{
    [Key]
    public Guid Id
    {
        get;
        set;
    } = Guid.NewGuid();

    public ulong UserId
    {
        get;
        set;
    }

    public ulong MessageId
    {
        get;
        set;
    }

    public ulong TextChannelId
    {
        get;
        set;
    }

    public DateTime CreatedAt
    {
        get;
        set;
    } = DateTime.Now;

    public CaffeineType Caffeine
    {
        get;
        set;
    }
}

public enum CaffeineType
{
    Coffee,
    Tea
}

public class UserData
{
    [Key]
    public ulong UserId
    {
        get;
        set;
    }

    public string UserName
    {
        get;
        set;
    }

    public string Mention
    {
        get;
        set;
    }
}

[PrimaryKey(nameof(JobId), nameof(ChannelId))]
public class JobData
{
    public string JobId
    {
        get;
        set;
    }

    public ulong ChannelId
    {
        get;
        set;
    }

    public string? ReminderJobId
    {
        get;
        set;
    }

    public string? Message
    {
        get;
        set;
    }

    [NotMapped] public string HangfireJobId => $"{ChannelId}-{JobId}";

    [NotMapped] public string HangfireReminderJobId => $"{HangfireJobId}-reminder";
}