using MinimalTelegramBot.Builder;

namespace StudyCompanion.Core.Contracts;

[Flags]
public enum CommandChat
{
    None = 0,
    Private = 0x01,
    Group = 0x02,
    All = Private | Group,
}

public record CommandDescription(string Command, string Description, CommandChat Chat);

public interface IBotCommand
{
    public static abstract List<CommandDescription> Commands { get; }

    public static abstract void ConfigureCommands(BotApplication bot);
    public static virtual void ConfigureCallbacks(BotApplication bot) { }
}
