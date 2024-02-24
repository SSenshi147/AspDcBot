namespace AspDcBot.Models;

public class Guild
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<Channel> Channels { get; set; } = [];
}
