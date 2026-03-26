using System;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.Polly;
using AspNetCore.Simple.Sdk.Serializer.Json;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;

namespace AspNetCore.Simple.Sdk.Caching
{
    internal static class AddRedisSettingsExtension
    {
        public static void AddRedisSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingletonOption<RedisSettings>(configuration);
        }
    }

    public record RedisSettings
    {
        public string HostName { get; init; } = string.Empty;

        public string ConnectionString { get; init; } = string.Empty;

        public TimeSpan AsyncTimeout { get; init; } = TimeSpan.FromSeconds(5);

        public TimeSpan SyncTimeout { get; init; } = TimeSpan.FromSeconds(5);

        public TimeSpan ConnectTimeout { get; init; } = TimeSpan.FromSeconds(5);

        public int ConnectRetry { get; init; } = 3;
    }

    public static class AddRedisCacheExtension
    {
        public static void AddRedisCache(this IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            if (configuration.TryGetSettings<RedisSettings>(out var redisSettings).IsFalse())
            {
#pragma warning disable CA1873
                logger.LogInformation($"No Redis settings was found. We activate InMemory caching service. For activating redis just add '{nameof(RedisSettings)}' to your appsettings or environment variables.{Environment.NewLine}Sample:{JToken.Parse(new RedisSettings().ToJson()).ToString(Formatting.Indented)}");
#pragma warning restore CA1873
                services.AddInMemoryCache();
                return;
            }

            if (services.IsAlreadyRegistered<RedisSettings>())
            {
                // Important do not connect redis twice
                return;
            }

            if (redisSettings.ConnectionString.IsNullOrWhiteSpace())
            {
                logger.LogInformation($"Connection string for Redis is missing, please check your configuration, secrets for '{nameof(RedisSettings)}__{nameof(RedisSettings.ConnectionString)}'. InMemory cache will be activated instead");
                services.AddInMemoryCache();
                return;
            }

            services.AddRedisConnectionFactory(configuration);

            // services.AddRedisConnection(configuration);
            var redisConnection = new RedisConnection(redisSettings, logger, new Backoff());
            redisConnection.StartupAsync().GetAwaiter().GetResult();

            services.AddSingletonIfNotExists<IRedisConnection>(redisConnection);
            services.AddSingletonIfNotExists<ICachingService, RedisCache>();
            services.AddCacheSettings(configuration);
        }
    }


    public class RedisCache(IRedisConnection redisConnection,
                            IJsonSerializer jsonSerializer,
                            CacheSettings cacheSettings) : ICachingService
    {
        private readonly TimeSpan _defaultTimInCache = TimeSpan.FromHours(1);

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
                return cachedObject with { CacheInfo = new CacheInfo(true, key, cachingTime) };
            }

            // If object was not in the cache, or it was invalid or structure changed we overwrite the cache and return newest value.
            return await SetAsync(key, itemFactory, cachingTime).ConfigureAwait(false);
        }

        public Task<bool> DeleteAsync(string cacheKey)
        {
            return redisConnection.ExecuteAsync(database => database.KeyDeleteAsync(new RedisKey(cacheKey)));
        }

        private async Task<T?> GetAsync<T>(string key) where T : class
        {
            var responseFromRedis = await redisConnection.ExecuteAsync(database => database.StringGetAsync(key)).ConfigureAwait(false);
            if (responseFromRedis.IsNull)
            {
                return default;
            }

            return responseFromRedis.HasValue ? jsonSerializer.Deserialize<T>(responseFromRedis!) : default;
        }

        private async Task<T> SetAsync<T>(string key, Func<Task<T>> itemFactory, TimeSpan cachingTime) where T : CachableObject
        {
            var item = await itemFactory().ConfigureAwait(false);
            await SetAsync(key, item, cachingTime).ConfigureAwait(false);

            var expirationDateTimeUtc = cacheSettings.WithExpirationDateTimeUtc ? await redisConnection.ExecuteAsync(database => database.KeyExpireTimeAsync(key)).ConfigureAwait(false) : default;
            return item with { CacheInfo = new CacheInfo(false, key, cachingTime, expirationDateTimeUtc ?? default) };
        }

        private Task SetAsync<T>(string key, T value, TimeSpan cachingTime)
        {
            var valueAsJson = jsonSerializer.Serialize(value);
            return redisConnection.ExecuteAsync(database => database.StringSetAsync(key, valueAsJson, cachingTime));
        }
    }
}
