using MinimalTelegramBot;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TParseMode = Telegram.Bot.Types.Enums.ParseMode;
using TType = Telegram.Bot.Types;
using StackExchange.Redis;
using StudyCompanion.Core.Extensions;

namespace StudyCompanion.Core.Builders;

public class ResultBuilder : MinimalTelegramBot.Results.IResult
{
    public static readonly bool USE_DELETE = false;
    public static readonly TParseMode DEFAULT_MODE = TParseMode.Html;

    public string? Text { get; set; }
    public bool Delete { get; set; } = false;
    public IReplyMarkup? ReplyMarkup { get; set; }
    public TParseMode ParseMode { get; set; } = TParseMode.None;
    public string? PhotoId { get; set; }

    public static implicit operator ResultBuilder(string text) => new()
    {
        Text = text,
    };

    public async Task ExecuteAsync(BotRequestContext context)
    {
        TType.Message? msg;

        if (PhotoId != null)
             msg = await context.Client.SendPhoto(context.ChatId, PhotoId, caption: Text, parseMode: ParseMode, replyMarkup: ReplyMarkup);
        else if (Text != null)
             msg = await context.Client.SendMessage(context.ChatId, Text, parseMode: ParseMode, replyMarkup: ReplyMarkup);
        else
            throw new ArgumentException("At least Text or Photo must be set");

        if (msg == null)
            return;

        // first check if we already have a state?
        // otherwise add new one
        
        // TODO change to using distributed cache

        if (USE_DELETE && Delete)
            await context.Services
                .GetRequiredService<IConnectionMultiplexer>()
                .GetDatabase()
                .ListRightPushAsync(context.ChatId.GetRedisKey(), msg.Id);
    }
}

public static class ResultBuilderExtensions
{
    public static ResultBuilder Delete(this string text) => new ResultBuilder() { Text = text }.Delete();
	public static ResultBuilder Delete(this ResultBuilder builder)
	{
		builder.Delete = true;
		return builder;
	}

    public static ResultBuilder AsMarkup(this string text, TParseMode? mode = null) => new ResultBuilder() { Text = text }.AsMarkup(mode);
	public static ResultBuilder AsMarkup(this ResultBuilder builder, TParseMode? mode = null)
	{
		builder.ParseMode = mode ?? ResultBuilder.DEFAULT_MODE;
        return builder;
	}

    public static ResultBuilder WithButtons(this string text, IReplyMarkup? buttons) => new ResultBuilder() { Text = text }.WithButtons(buttons);
	public static ResultBuilder WithButtons(this ResultBuilder builder, IReplyMarkup? buttons)
	{
		builder.ReplyMarkup = buttons;
        return builder;
	}

    public static ResultBuilder WithPhoto(this string text, string? photoId) => new ResultBuilder() { Text = text }.WithPhoto(photoId);
	public static ResultBuilder WithPhoto(this ResultBuilder builder, string? photoId)
	{
        builder.PhotoId = photoId;
        return builder;
	}
}

