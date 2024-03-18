namespace DonDumbledore.Logic;

public sealed class DonDumbledoreConfig
{
    public string SqliteConnectionString { get; set; }
    public string HangfireSqliteConnectionString { get; set; }
    public string BotToken { get; set; }
    public bool DeleteAndReRegisterGuildCommands { get; set; }
    public bool DeleteAndReRegisterGlobalCommands { get; set; }
}
