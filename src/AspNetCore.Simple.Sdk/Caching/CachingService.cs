using System;
using System.Threading.Tasks;

namespace AspNetCore.Simple.Sdk.Caching
{
    public record CachableObject
    {
        public CacheInfo CacheInfo { get; init; } = new();
    }

    public record CacheInfo(bool ObjectFromCache = false,
                            string CacheKey = "",
                            TimeSpan CacheTime = default,
                            DateTime ExpirationDateTimeUtc = default);

    public interface ICachingService
    {
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> itemFactory, bool useCache) where T : CachableObject;

        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> itemFactory, bool useCache, TimeSpan cachingTime) where T : CachableObject;

        Task<bool> DeleteAsync(string cacheKey);
    }
}
