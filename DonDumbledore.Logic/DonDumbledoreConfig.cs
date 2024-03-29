namespace DonDumbledore.Logic;

public sealed class DonDumbledoreConfig
{
    public bool IsProductionEnvironment { get; set; }
    public bool RegisterNewCommands { get; set; }

    public string SqliteConnectionString { get; set; }
    public string HangfireSqliteConnectionString { get; set; }
    public string BotToken { get; set; }
}
