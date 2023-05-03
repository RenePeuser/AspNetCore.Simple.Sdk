using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Caching
{
    public static class AddCacheSettingsExtension
    {
        public static void AddCacheSettings(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.TryGetSettings<CacheSettings>(out var cacheSettings))
            {
                services.AddSingletonIfNotExists(cacheSettings);
            }


            services.AddSingletonIfNotExists(new CacheSettings());
        }
    }

    public class CacheSettings
    {
        public bool WithExpirationDateTimeUtc { get; init; }
    }
}
