using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    public static class ErrorHandlingExtensions
    {
        public static void AddErrorHandling(this IServiceCollection services)
        {
            services.AddSingleton<IErrorHandlingStrategy, ErrorHandlingStrategy>();

            services.AddSingleton<ErrorHandlingMiddleware>();

            services.AddSingleton<ISpecificErrorHandler, SecurityProblemExceptionHandler>();
            services.AddSingleton<ISpecificErrorHandler, ProblemDetailsExceptionHandler>();

            // Must be the last one, order of registrations means priority of handling exception type !!!
            services.AddSingleton<ISpecificErrorHandler, DefaultExceptionHandler>();
        }

        public static void UseErrorHandling(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
