using ExpenseTracker.Api.Interface;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ExpenseTracker.Api.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;
        private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(30);
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }
        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            var cached = await _cache.GetStringAsync(key);
            if (cached is not null)
            {
                _logger.LogInformation("Cache hit for {CacheKey}", key);
                return JsonSerializer.Deserialize<T>(cached, JsonOptions)!;
            }
            _logger.LogInformation("Cache miss for {CacheKey} - loading from source",key);
            var value = await factory();
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? DefaultExpiry
            };
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(value, JsonOptions), options);
            return value;
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
            _logger.LogInformation("Cache entry {CacheKey} removed", key);
        }
    }
}
