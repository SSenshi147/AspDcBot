using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AspDcBot.Data;

public class BotDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<DrinkModel> DrinkModels { get; set; }
}

public class DrinkModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public ulong UserId { get; set; }
    public ulong MessageId { get; set; }
    public ulong TextChannelId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public CaffeineType Caffeine { get; set; }
}

public enum CaffeineType
{
    Coffee, Tea
}