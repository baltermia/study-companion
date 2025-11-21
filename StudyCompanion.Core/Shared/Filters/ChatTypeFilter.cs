using MinimalTelegramBot.Handling;
using MinimalTelegramBot.Handling.Filters;
using Telegram.Bot.Types.Enums;

namespace StudyCompanion.Core.Shared.Filters;

internal static class ChatTypeFilter
{
    public static THandler FilterChatType<THandler>(this THandler builder, params ChatType[] types)
        where THandler : IHandlerConventionBuilder =>
        builder.Filter(context => 
            context.BotRequestContext.Update.Message?.Chat.Type is ChatType type && types.Contains(type));
}

