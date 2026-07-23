namespace ExpenseTracker.Api.Interface
{
    public interface ICacheService
    {
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);
        Task RemoveAsync(string key);
    }
}
