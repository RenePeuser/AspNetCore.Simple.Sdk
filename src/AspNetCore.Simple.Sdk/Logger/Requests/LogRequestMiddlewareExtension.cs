using Extensions.Pack;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Logger.Requests
{
    public static class LogRequestMiddlewareExtension
    {
        public static void AddLogRequestMiddleware(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingletonIfNotExists<LogRequestMiddleware>();
        }

        public static void UseLogRequestMiddleware(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware<LogRequestMiddleware>();
        }
    }
}
