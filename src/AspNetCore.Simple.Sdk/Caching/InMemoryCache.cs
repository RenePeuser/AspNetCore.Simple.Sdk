using System;
using System.Threading.Tasks;
using Extensions.Pack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Caching
{
    public static class AddInMemoryCacheExtension
    {
        public static void AddInMemoryCache(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingletonIfNotExists<ICachingService, InMemoryCache>();
        }
    }
    internal sealed class InMemoryCache : ICachingService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly TimeSpan _defaultTimInCache = TimeSpan.FromHours(1);

        public InMemoryCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> itemFactory, bool useCache) where T : CachableObject
        {
            return GetOrAddAsync(key, itemFactory, useCache, _defaultTimInCache);
        }

        public async Task<T> GetOrAddAsync<T>(string key,
                                              Func<Task<T>> itemFactory,
                                              bool useCache,
                                              TimeSpan cachingTime) where T : CachableObject
        {
            // Force no cache usage
            if (useCache.IsFalse())
            {
                return await SetCacheAsync(key, itemFactory, cachingTime).ConfigureAwait(false);
            }

            // Try get object out of cache, if not possible set current value and return actual one
            if (_memoryCache.TryGetValue(key, out var result).IsFalse())
            {
                return await SetCacheAsync(key, itemFactory, cachingTime).ConfigureAwait(false);
            }

            // If type from cache is not expected one we update the old value with the new one and return the current value.
            if (result is not T typedResult)
            {
                return await SetCacheAsync(key, itemFactory, cachingTime).ConfigureAwait(false);
            }

            // Important we return the object from cache with all cache infos.
            var cacheResult = typedResult with
            {
                CacheInfo = typedResult.CacheInfo with { ObjectFromCache = true, CacheKey = key }
            };

            return cacheResult;

        }

        public Task<bool> DeleteAsync(string cacheKey)
        {
            _memoryCache.Remove(cacheKey);
            return Task.FromResult(true);
        }

        private async Task<T> SetCacheAsync<T>(string key,
                                               Func<Task<T>> itemFactory,
                                               TimeSpan cachingTime) where T : CachableObject
        {
            var item = await itemFactory().ConfigureAwait(false);
            _memoryCache.Set(key, item, cachingTime);
            return item with { CacheInfo = item.CacheInfo with { CacheKey = key, ObjectFromCache = false } };
        }

    }
}
