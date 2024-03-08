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
    public ulong UserId { get; set; }
    public int CoffeCount { get; set; }
    public int TeaCount { get; set; }
}