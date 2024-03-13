using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DonDumbledore.Logic.Data;

public class BotDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<DrinkModel> DrinkModels { get; set; }
    public DbSet<UserData> UserDataModels { get; set; }
}

public class DrinkModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public ulong UserId { get; set; }
    public ulong MessageId { get; set; }
    public ulong TextChannelId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public CaffeineType Caffeine { get; set; }
}

public enum CaffeineType
{
    Coffee,
    Tea
}

public class UserData
{
    [Key]
    public ulong UserId { get; set; }

    public string UserName { get; set; }
    public string Mention { get; set; }
}