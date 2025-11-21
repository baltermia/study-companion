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
            InlineKeyboardButton.WithCallbackData("✔️ " + lang.GetLocalized(de => "Ja", en => "Yes"), prefix + "yes"),
            InlineKeyboardButton.WithCallbackData("❌ " + lang.GetLocalized(de => "Nein", en => "No"), prefix + "no"),
        ]],
    };
}