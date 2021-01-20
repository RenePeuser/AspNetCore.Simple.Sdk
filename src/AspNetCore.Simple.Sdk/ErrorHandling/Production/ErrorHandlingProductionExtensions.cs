using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ErrorHandling.Production
{
    public static class ErrorHandlingProductionExtensions
    {
        public static void AddErrorHandlingProduction(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ErrorHandlingProductionMiddleware>();
            serviceCollection.AddSingleton<DefaultExceptionHandler>();
            serviceCollection.AddSingleton<ProblemDetailsExceptionHandler>();
            serviceCollection.AddSingleton<UserResponseExceptionHandler>();
        }

        public static void UseErrorHandlingProduction(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware<ErrorHandlingProductionMiddleware>();
        }
    }
}
