using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Extensions
{
    public static class AddHttpClientFactoryExtension
    {
        public static void AddHttpClientFactory(this IServiceCollection services)
        {
            services.AddHttpClient();
        }
    }
}
