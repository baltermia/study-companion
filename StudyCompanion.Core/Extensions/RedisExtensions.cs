using StackExchange.Redis;

namespace StudyCompanion.Core.Extensions;

public static class RedisExtensions
{
    public static async Task<KeyTransaction> CreateTransactionAsync(this IDatabase database, string key, string dummyValue = "")
    {
        bool exists = await database.KeyExistsAsync(key);

        if (!exists)
            await database.StringSetAsync(key, dummyValue);

        return new KeyTransaction(database, key, exists);
    }
}

/// <summary>
/// Can be used to ensure that a code context is only called once until the key gets dropped.
/// This should only be used with await using statements.
/// 
/// If a key already exists, nothing will happen and the <see cref="KeyExists"/> property will be true.
/// If not, the key will be created, the <see cref="KeyExists"/> property will be false and after being disposed
/// the key gets dropped again.
/// </summary>
public struct KeyTransaction(IDatabase db, string key, bool exists) : IAsyncDisposable
{
    public bool KeyExists { get; } = exists;

    public async ValueTask DisposeAsync()
    {
        if (!KeyExists)
            await db.KeyDeleteAsync(key);
    }
}
