using System;
using System.Threading.Tasks;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{

    public static class AddErrorHandlingMiddlewareExtension
    {
        public static void AddErrorHandlingMiddleware(this IServiceCollection services)
        {
            services.AddErrorHandlingStrategy();

            services.AddSingletonIfNotExists<ErrorHandlingMiddleware>();
        }
    }


    // this middle ware is just for handling the errors, not for logging !!
    internal sealed class ErrorHandlingMiddleware : IMiddleware
    {
        private readonly IErrorHandlingStrategy _errorHandlingStrategy;

        public ErrorHandlingMiddleware(IErrorHandlingStrategy errorHandlingStrategy)
        {
            _errorHandlingStrategy = errorHandlingStrategy;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await _errorHandlingStrategy.HandleAsync(context, exception).ConfigureAwait(false);
            }
        }
    }
}
