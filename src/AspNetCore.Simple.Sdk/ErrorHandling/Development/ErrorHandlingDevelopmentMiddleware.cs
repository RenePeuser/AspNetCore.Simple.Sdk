using System;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.ErrorHandling.Production;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.ErrorHandling.Development
{
    internal class ErrorHandlingDevelopmentMiddleware : IMiddleware
    {
        private readonly ProblemDetailsExceptionHandler _problemDetailsExceptionHandler;
        private readonly DefaultExceptionHandler _defaultExceptionHandler;
        private readonly UserResponseExceptionHandler _userResponseExceptionHandler;

        public ErrorHandlingDevelopmentMiddleware(ProblemDetailsExceptionHandler problemDetailsExceptionHandler, DefaultExceptionHandler defaultExceptionHandler, UserResponseExceptionHandler userResponseExceptionHandler)
        {
            _problemDetailsExceptionHandler = problemDetailsExceptionHandler;
            _defaultExceptionHandler = defaultExceptionHandler;
            _userResponseExceptionHandler = userResponseExceptionHandler;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context).ConfigureAwait(false);
            }
            catch (UserResponseException userResponseException)
            {
                await _userResponseExceptionHandler.HandleAsync(context, userResponseException).ConfigureAwait(false);
            }
            catch (ProblemDetailsException exception)
            {
                await _problemDetailsExceptionHandler.HandleAsync(context, exception).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await _defaultExceptionHandler.HandleAsync(context, exception).ConfigureAwait(false);
            }
        }
    }
}
