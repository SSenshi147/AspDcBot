namespace AspDcBot.Commands;

[AttributeUsage(AttributeTargets.Class)]
public class SlashCommandInfoAttribute : Attribute
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}
