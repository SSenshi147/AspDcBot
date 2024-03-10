namespace DonDumbledore.Logic.Commands;

[AttributeUsage(AttributeTargets.Class)]
public class SlashCommandInfoAttribute(string name, string description) : Attribute
{
    public string Name { get; } = name;
    public string Description { get; } = description;
}