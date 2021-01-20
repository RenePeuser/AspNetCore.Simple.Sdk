using AspNetCore.Simple.Sdk.ErrorHandling.Production;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ErrorHandling.Development
{
    public static class ErrorHandlingDevelopmentExtensions
    {
        public static void AddErrorHandlingDevelopment(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ErrorHandlingDevelopmentMiddleware>();
            serviceCollection.AddSingleton<DefaultExceptionHandler>();
            serviceCollection.AddSingleton<ProblemDetailsExceptionHandler>();
            serviceCollection.AddSingleton<UserResponseExceptionHandler>();
        }

        public static void UseErrorHandlingDevelopment(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware<ErrorHandlingDevelopmentMiddleware>();
        }
    }
}
