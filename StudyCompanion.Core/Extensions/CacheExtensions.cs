using Microsoft.Extensions.Caching.Distributed;

namespace StudyCompanion.Core.Extensions;

public static class CacheExtensions
{
    public static async Task<KeyTransaction> CreateTransactionAsync(this IDistributedCache cache, string key, string dummyValue = "")
    {
        string? str = await cache.GetStringAsync(key);
        
        bool exists = str != null;

        if (!exists)
            await cache.SetStringAsync(key, dummyValue);

        return new KeyTransaction(cache, key, exists);
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
public struct KeyTransaction(IDistributedCache cache, string key, bool exists) : IAsyncDisposable
{
    public bool KeyExists { get; } = exists;

    public async ValueTask DisposeAsync()
    {
        if (!KeyExists)
            await cache.RemoveAsync(key);
    }
}
