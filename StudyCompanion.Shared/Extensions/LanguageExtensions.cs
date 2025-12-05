using System.Globalization;
using StudyCompanion.Shared.Models;

namespace StudyCompanion.Shared.Extensions;

public static class LanguageExtensions
{
    public static string GetLocalized(this Language language, 
        Func<Language, string>? en,
        Func<Language, string> de) 
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

    private static readonly Dictionary<Language, CultureInfo> _cultures = new()
    {
        [Language.English] = new CultureInfo("en-GB"),
        [Language.German] = new CultureInfo("de-DE"),
    };

    public static CultureInfo ToCultureInfo(this Language language) => _cultures[language];
}

