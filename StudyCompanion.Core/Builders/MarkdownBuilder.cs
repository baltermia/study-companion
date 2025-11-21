namespace StudyCompanion.Core.Builders;

public enum ParseMode
{
    Html,
    Markdown,
}

public static class MarkdownBuilderExtensions
{
    public static MarkdownBuilder Bold(this string content) => new MarkdownBuilder(content).Bold();
    public static MarkdownBuilder Bold(this MarkdownBuilder builder)
    {
        if (!builder.Elements.OfType<BoldMarkdown>().Any())
            builder.Elements.Add(new BoldMarkdown());

        return builder;
    }

    public static MarkdownBuilder Link(this string content, string url) => new MarkdownBuilder(content).Link(url);
    public static MarkdownBuilder Link(this MarkdownBuilder builder, string url)
    {
        if (!builder.Elements.OfType<LinkMarkdown>().Any())
            builder.Elements.Add(new LinkMarkdown(url));

        return builder;
    }

    public static MarkdownBuilder Code(this string content) => new MarkdownBuilder(content).Code();
    public static MarkdownBuilder Code(this MarkdownBuilder builder)
    {
        if (!builder.Elements.OfType<CodeMarkdown>().Any())
            builder.Elements.Add(new CodeMarkdown());

        return builder;
    }

    public static MarkdownBuilder Newline(this string content, int? count = null) => new MarkdownBuilder(content).Newline(count);
    public static MarkdownBuilder Newline(this MarkdownBuilder builder, int? count = null)
    {
        builder.Elements.Add(new NewlineMarkdown()
        {
            Count = count,
        });

        return builder;
    }

    public static MarkdownBuilder Fill(this string content, char character, int length) => new MarkdownBuilder(content).Fill(character, length);
    public static MarkdownBuilder Fill(this MarkdownBuilder builder, char character, int length)
    {
        builder.Elements.Add(new FillMarkdown
        {
            Character = character,
            Length = length,
        });

        return builder;
    }

    public static MarkdownBuilder With(this MarkdownBuilder builder, ParseMode mode)
    {
        builder.Mode = mode;
        return builder;
    }
}

public abstract class MarkdownElement
{
    public abstract string Apply(string content, ParseMode mode);
}

public class CodeMarkdown : MarkdownElement
{
    public override string Apply(string content, ParseMode mode) => mode switch
    {
        ParseMode.Html => $"<code>{content}</code>",
        ParseMode.Markdown => $"`{content}`",
        _ => content,
    };
}

public class BoldMarkdown : MarkdownElement
{
    public override string Apply(string content, ParseMode mode) => mode switch
    {
        ParseMode.Html => $"<b>{content}</b>",
        ParseMode.Markdown => $"**{content}**",
        _ => content,
    };
}

public class LinkMarkdown(string url) : MarkdownElement
{
    public string Url { get; } = url;

    public override string Apply(string content, ParseMode mode) => mode switch
    {
        ParseMode.Html => $"<a href={Url}>{content}</a>",
        ParseMode.Markdown => $"[{content}]({Url})",
        _ => content,
    };
}

public class NewlineMarkdown : MarkdownElement
{
    public int? Count { get; set; }

    public override string Apply(string content, ParseMode mode) =>
        Count > 1
        ? content + string.Concat(Enumerable.Repeat(Environment.NewLine, Count.Value))
        : content + Environment.NewLine;
}

/// <summary>
/// Fills the content with remaining character until length has been reached
/// </summary>
public class FillMarkdown : MarkdownElement
{
    public required char Character { get; set; }
    public required int Length { get; set; }

    public override string Apply(string content, ParseMode mode)
    {
        int missing = Length - content.Length;

        return missing > 0 
            ? content + new string(Character, missing) 
            : content;
    }
}

public class MarkdownBuilder(string content)
{
    public static ParseMode DefaultParseMode = ParseMode.Html;

    public string Content { get; } = content;
    public ParseMode? Mode { get; set; } = null;
    public ICollection<MarkdownElement> Elements { get; } = [];

    public static implicit operator string(MarkdownBuilder builder) => builder.ToString();

    public static implicit operator MarkdownBuilder(string text) => new(text);

    public override string ToString() => ToString(null);

    public string ToString(ParseMode? mode = null)
    {
        string result = Content;

        foreach (MarkdownElement element in Elements)
            result = element.Apply(result, mode ?? Mode ?? DefaultParseMode);

        return result;
    }
}
