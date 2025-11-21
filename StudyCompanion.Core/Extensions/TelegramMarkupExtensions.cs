using StudyCompanion.Core.Builders;
using TParseMode = Telegram.Bot.Types.Enums.ParseMode;

namespace StudyCompanion.Core.Extensions;

public static class TelegramMarkupExtensions
{
    public static MarkdownBuilder With(this MarkdownBuilder builder, TParseMode mode)
    {
        builder.Mode = mode switch
        {
            TParseMode.Html => ParseMode.Html,
            _ => ParseMode.Markdown,
        };

        return builder;
    }
}
