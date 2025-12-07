using MinimalTelegramBot;
using MinimalTelegramBot.Builder;
using MinimalTelegramBot.Handling;
using StudyCompanion.Core.Builders;
using StudyCompanion.Core.Contracts;
using StudyCompanion.Core.Extensions;
using StudyCompanion.Core.Shared.Filters;
using StudyCompanion.Shared.Contracts;
using StudyCompanion.Shared.Extensions;
using StudyCompanion.Shared.Models;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using IResult = MinimalTelegramBot.Results.IResult;
using Message = Telegram.Bot.Types.Message;
using ParseMode = Telegram.Bot.Types.Enums.ParseMode;
using Results = MinimalTelegramBot.Results.Results;

namespace StudyCompanion.Core.Commands;

public class SummaryCommand : IBotCommand
{
    public static string GetTitle(Language lang) =>
        lang.GetLocalized(en => "🔍 AI Summary", de => "🔍 AI Zusammenfassung");
    
    public static List<CommandDescription> Commands { get; } =
    [
        new("/summary", "🔍 AI Summary", CommandChat.Private),
    ];

    public static void ConfigureCommands(BotApplication bot)
    {
        bot.HandleCommand("/summary", OnSummary)
            .FilterChatType(ChatType.Private);

        bot.HandleMessageText(GetTitle(Language.English), OnSummary)
            .FilterChatType(ChatType.Private);

        bot.HandleMessageText(GetTitle(Language.German), OnSummary)
            .FilterChatType(ChatType.Private);
    }

    private static async Task<IResult> OnSummary(BotRequestContext context, IHelper helper, IAiService ai)
    {
        await context.DropPrevious();

        if (await helper.GetUserAsync(context.ChatId, true) is not User user)
            return Results.Empty;

        Language lang = user.Settings.Language;

        int? msgId = null;
        
        try
        {
            Message msg = await context.Client.SendMessage(context.ChatId, lang.GetLocalized(
                en => "⏳ Give me a second...",
                de => "⏳ Einen Moment bitte..."
            ));
            
            msgId = msg.MessageId;
            
            await context.Client.SendChatAction(context.ChatId, ChatAction.Typing);

            string summary = await ai.GetUserSummary(user);
            
            await context.Client.DeleteMessage(context.ChatId, msgId.Value);
            msgId = null;
            
            string text = GetTitle(lang).Bold().Newline().Newline() + summary;

            return text.AsMarkup().Delete();
        }
        finally
        {
            if (msgId is int valid)
                await context.Client.DeleteMessage(context.ChatId, valid);
        }
    }
}