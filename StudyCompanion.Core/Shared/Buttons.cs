using StudyCompanion.Shared.Extensions;
using StudyCompanion.Shared.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace StudyCompanion.Core.Shared;

public static class Buttons
{
    public static InlineKeyboardMarkup YesNoKeyboard(string prefix, Language lang) => new()
    {
        InlineKeyboard = 
        [[
            InlineKeyboardButton.WithCallbackData("✔️ " + lang.GetLocalized(en => "Yes", de => "Ja"), prefix + "yes"),
            InlineKeyboardButton.WithCallbackData("❌ " + lang.GetLocalized(en => "No", de => "Nein"), prefix + "no"),
        ]],
    };
}