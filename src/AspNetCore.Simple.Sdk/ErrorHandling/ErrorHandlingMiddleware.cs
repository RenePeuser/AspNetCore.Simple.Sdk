using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    // this middle ware is just for handling the errors, not for logging !!
    internal class ErrorHandlingMiddleware : IMiddleware
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
