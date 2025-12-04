using MinimalTelegramBot;
using MinimalTelegramBot.Builder;
using MinimalTelegramBot.Handling;
using StudyCompanion.Core.Builders;
using StudyCompanion.Core.Contracts;
using StudyCompanion.Core.Extensions;
using StudyCompanion.Core.Shared.Filters;
using StudyCompanion.Shared.Extensions;
using StudyCompanion.Shared.Models;
using StudyCompanion.Shared.Services;
using IResult = MinimalTelegramBot.Results.IResult;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace StudyCompanion.Core.Commands;

internal class Start : IBotCommand
{
    public static List<CommandDescription> Commands { get; } =
    [
        new("/start", "🏠 Bot Start Dialog", CommandChat.Private),
    ];

    public static ReplyKeyboardMarkup GetButtons(Language lang, Role? role)
    {
        List<KeyboardButton> row1 =
        [
            new("🏠 Home"),
        ];

        List<KeyboardButton> row2 =
        [
            new("⚙️ Einstellungen"),
        ];

        if (role is Role.Mod or Role.Admin)
            row2.Add(new KeyboardButton("🛡️ Admin"));

        return new()
        {
            Keyboard = [row1, row2],
            ResizeKeyboard = true,
            IsPersistent = true,
        };
    }

    public static void ConfigureCommands(BotApplication bot)
    {
        bot.HandleCommand("/start", OnStart)
            .FilterChatType(ChatType.Private);

        // TODO after handling start command (for referral)
        // we could add a generic handler which will always
        // ensure that a user exists

        bot.HandleMessageText("🏠 Home", OnStart)
            .FilterChatType(ChatType.Private);
    }

    private static async Task<IResult> OnStart(BotRequestContext context, IHelper helper)
    {
        await context.DropPrevious();

        User? user = null;

        // ensures that the account gets created
        if (context.Update.Message?.ConvertMessage() is Message msg && msg.Chat is TelegramUser telegramUser)
        {
            user = await helper.GetUserAsync(telegramUser);
        }

        Language lang = user?.Settings.Language ?? Language.English;

        string text = lang.GetLocalized(
            de => "Wilkommen, ich bin dein Study Companion!".Bold().Newline(),
            en => "Welcome to your Study Companion!".Bold().Newline() 
        );

        if (user?.Settings.Calender?.Link != null)
        {
            // ical is needed
            
            text += lang.GetLocalized(
                de => """
                      Erstmals brauche ich deinen iCal Kalender. Antworte dazu einfach mit dem Link.
                      
                      Sprache ändern / Change language: /settings
                      """,
                de => """
                      First off I need your iCal Calender. Simply respond with the link.

                      Change Language / Sprache ändern: /settings
                      """
            );
        }
        else
        {
            
        }

        return 
            text.AsMarkup()
                .WithButtons(GetButtons(lang, user?.Role))
                .Delete();
    }
}


