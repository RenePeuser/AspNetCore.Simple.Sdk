using System;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.Extensions;
using AspNetCore.Simple.Sdk.Serializer.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace AspNetCore.Simple.Sdk.Caching
{
    public record Redis
    {
        public string HostName { get; init; } = string.Empty;

        public string ConnectionString { get; init; } = string.Empty;
    }

    public static class AddRedisCacheExtension
    {
        public static void AddRedisCache(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var redisSettings = configuration.GetSetting<Redis>();
            try
            {
                var connection = ConnectionMultiplexer.Connect(redisSettings.ConnectionString);
                var database = connection.GetDatabase();
                serviceCollection.AddSingleton(database);
                serviceCollection.AddSingleton<ICachingService, RedisCache>();
                Console.WriteLine($"Connection to Redis endpoint: '{redisSettings.HostName}' was successful. Hostname: '{redisSettings.HostName}'");
            }
            catch (Exception e)
            {
                Console.WriteLine($"No connection could be established to Redis endpoint: '{redisSettings.HostName}', dummy cache without caching will be created. Exception message: {e.Message}.");
                serviceCollection.AddInMemoryCache();
            }
        }
    }
    public class RedisCache : ICachingService
    {
        private readonly IDatabase _database;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly TimeSpan _defaultTimInCache = TimeSpan.FromHours(1);

        public RedisCache(IDatabase database,
                          IJsonSerializer jsonSerializer)
        {
            _database = database;
            _jsonSerializer = jsonSerializer;
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
            // If flag force to not use the cache, then we set the newest value into the cache, a kind of invalidation too.
            if (!useCache)
            {
                return await SetAsync(key, itemFactory, cachingTime).ConfigureAwait(false);
            }

            // Try to get object from cache
            var cachedObject = await GetAsync<T>(key).ConfigureAwait(false);

            // If object comes from cache, we will attach cache info what is needed
            if (cachedObject is not null)
            {
                return cachedObject with { CacheInfo = new CacheInfo(true, key) };
            }

            // If object was not in the cache, or it was invalid or structure changed we overwrite the cache and return newest value.
            return await SetAsync(key, itemFactory, cachingTime).ConfigureAwait(false);
        }

        public Task<bool> DeleteAsync(string cacheKey)
        {
            return _database.KeyDeleteAsync(new RedisKey(cacheKey));
        }

        private async Task<T?> GetAsync<T>(string key) where T : class
        {
            var responseFromRedis = await _database.StringGetAsync(key).ConfigureAwait(false);
            return responseFromRedis.HasValue ? _jsonSerializer.Deserialize<T>(responseFromRedis) : default;
        }

        private async Task<T> SetAsync<T>(string key, Func<Task<T>> itemFactory, TimeSpan cachingTime) where T : CachableObject
        {
            var item = await itemFactory().ConfigureAwait(false);
            await SetAsync(key, item, cachingTime).ConfigureAwait(false);
            return item with { CacheInfo = new CacheInfo(false, key) };
        }

        private Task SetAsync<T>(string key, T value, TimeSpan cachingTime)
        {
            var valueAsJson = _jsonSerializer.Serialize(value);
            return _database.StringSetAsync(key, valueAsJson, cachingTime);
        }
    }
}
