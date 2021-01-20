using AspNetCore.Simple.Sdk.Logger.Errors.Strategies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Logger.Errors.Middlewares
{
    public static class ErrorLoggingMiddlewareExtension
    {
        public static void AddErrorLogging(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogStrategies();

            serviceCollection.AddSingleton<ErrorLoggingMiddleware>();
        }

        public static void UseErrorLogging(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware<ErrorLoggingMiddleware>();
        }
    }
}