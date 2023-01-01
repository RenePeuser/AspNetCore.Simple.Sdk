using Extensions.Pack;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public static class ErrorLoggingMiddlewareExtension
    {
        public static void AddErrorLogging(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogStrategies();

            serviceCollection.AddSingletonIfNotExists<ErrorLoggingMiddleware>();
        }

        public static void UseErrorLogging(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware<ErrorLoggingMiddleware>();
        }
    }
}
