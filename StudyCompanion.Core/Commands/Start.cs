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

        User? player = null;

        // ensures that the account gets created
        if (context.Update.Message?.ConvertMessage() is Message msg && msg.Chat is TelegramUser user)
        {
            long? id = null;

            if (!string.IsNullOrWhiteSpace(msg.Text))
            {
                string[] args = msg.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (args.Length >= 2 && args[1].StartsWith("ref"))
                {
                    string[] splits = args[1].Split("_");

                    if (splits.Length >= 2 && long.TryParse(splits[1], out long result))
                        id = result;
                }
            }

            player = await helper.GetPlayerAsync(user);
        }

        Language lang = player?.Settings.Language ?? Language.English;

        string text = lang.GetLocalized(
            de => "Wilkommen, ich bin dein Study Companion!".Bold().Newline() 
        );

        return 
            text.AsMarkup()
                .WithButtons(GetButtons(lang, player?.Role))
                .Delete();
    }
}


