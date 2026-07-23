using ExpenseTracker.Api.Interface;

namespace ExpenseTracker.Api.Tests;

// A pass-through cache for tests: it never caches, it just runs the factory every time.
// That keeps tests deterministic (they always exercise the real DB query path) and free of
// any Redis dependency. RemoveAsync is a no-op since there's nothing to evict.
internal sealed class FakeCacheService : ICacheService
{
    public Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null) => factory();

    public Task RemoveAsync(string key) => Task.CompletedTask;
}
