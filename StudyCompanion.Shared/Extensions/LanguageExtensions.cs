using StudyCompanion.Shared.Models;

namespace StudyCompanion.Shared.Extensions;

public static class LanguageExtensions
{
    public static string GetLocalized(this Language language, 
        Func<Language, string> de, 
        Func<Language, string>? en = null) 
        => language switch
    {
        Language.English => en?.Invoke(language) ?? de(language),
        Language.German => de(language),
        //_ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
        _ => de(language) // currently default to German
    };

    public static string ToLanguageString(this Language language) => language switch
    {
        Language.English => "🇬🇧 English",
        Language.German => "🇩🇪 Deutsch",
        _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
    };
}

