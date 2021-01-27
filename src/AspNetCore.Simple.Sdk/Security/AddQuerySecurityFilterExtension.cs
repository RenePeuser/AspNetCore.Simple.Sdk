using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Security
{
    public static class AddQuerySecurityFilterExtension
    {
        public static void AddQuerySecurityFilter(this IServiceCollection services)
        {
            services.AddMvc(config => config.Filters.Add<QuerySecurityFilter>());
        }
    }
}