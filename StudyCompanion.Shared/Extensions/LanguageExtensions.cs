using StudyCompanion.Shared.Models;

namespace StudyCompanion.Shared.Extensions;

public static class LanguageExtensions
{
    public static string GetLocalized(this Language language, Func<Language, string> en, Func<Language, string> de) => language switch
    {
        Language.English => en(language),
        Language.German => de(language),
        _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
    };

    public static string ToLanguageString(this Language language) => language switch
    {
        Language.English => "🇬🇧 English",
        Language.German => "🇩🇪 Deutsch",
        _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
    };
}

