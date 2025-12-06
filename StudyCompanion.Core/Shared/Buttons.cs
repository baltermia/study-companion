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
    
    public static bool? ParseYesNoCallback(string? data, string prefix)
    {
        if (data == prefix + "yes")
            return true;
        
        if (data == prefix + "no")
            return false;
        
        return null;
    }
}