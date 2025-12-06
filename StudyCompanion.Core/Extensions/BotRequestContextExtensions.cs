using Microsoft.Extensions.Caching.Distributed;
using StudyCompanion.Core.Builders;
using MinimalTelegramBot;
using MinimalTelegramBot.StateMachine.Extensions;
using Telegram.Bot;

namespace StudyCompanion.Core.Extensions;

public static class BotRequestContextExtensions
{
    public static async Task DropPrevious(this BotRequestContext context)
    {
        await context.DropState();

        if (!ResultBuilder.USE_DELETE)
            return;

        IDistributedCache cache = context.Services.GetRequiredService<IDistributedCache>();

        string key = context.ChatId.GetRedisKey();

        List<int> msgs = await cache.GetMessageIdsAsync(key);

        if (msgs.Count == 0)
            return;

        await context.Client.DeleteMessages(context.ChatId, msgs);

        await cache.RemoveAsync(key);
    }
}

public static class MessageStateExtensions
{
    public static readonly string REDIS_PREFIX = "StudyCompanion:messageState:";
    public static string GetRedisKey(this long chatId) => $"{REDIS_PREFIX}{chatId}";
}

