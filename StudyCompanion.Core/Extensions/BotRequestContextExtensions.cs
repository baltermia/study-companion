using StudyCompanion.Core.Builders;
using MinimalTelegramBot;
using MinimalTelegramBot.StateMachine.Extensions;
using StackExchange.Redis;
using Telegram.Bot;

namespace StudyCompanion.Core.Extensions;

public static class BotRequestContextExtensions
{
    public static async Task DropPrevious(this BotRequestContext context)
    {
        await context.DropState();

        if (!ResultBuilder.USE_DELETE)
            return;

        IDatabase db = context.Services.GetRequiredService<IConnectionMultiplexer>().GetDatabase();

        string key = context.ChatId.GetRedisKey();

        RedisValue[] values = await db.ListRangeAsync(key);

        if (values.Length == 0)
            return;

        int[] messageIds = Array.ConvertAll(values, item => (int)item);

        await context.Client.DeleteMessages(context.ChatId, messageIds);

        await db.KeyDeleteAsync(key);
    }
}

public static class MessageStateExtensions
{
    public static readonly string REDIS_PREFIX = "StudyCompanion:messageState:";
    public static string GetRedisKey(this long chatId) => $"{REDIS_PREFIX}{chatId}";
}

