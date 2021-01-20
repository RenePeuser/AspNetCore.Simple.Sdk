using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.ErrorHandling.Production
{
    internal class ErrorHandlingProductionMiddleware : IMiddleware
    {
        private readonly ProblemDetailsExceptionHandler _problemDetailsExceptionHandler;
        private readonly DefaultExceptionHandler _defaultExceptionHandler;
        private readonly UserResponseExceptionHandler _userResponseExceptionHandler;

        public ErrorHandlingProductionMiddleware(ProblemDetailsExceptionHandler problemDetailsExceptionHandler, DefaultExceptionHandler defaultExceptionHandler, UserResponseExceptionHandler userResponseExceptionHandler)
        {
            _problemDetailsExceptionHandler = problemDetailsExceptionHandler;
            _defaultExceptionHandler = defaultExceptionHandler;
            _userResponseExceptionHandler = userResponseExceptionHandler;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // ToDo: talk about exception handling, how we want to handle it may
            // because if we want to register different strategies for different exception
            // we need more classes here => with a set of strategies.
            try
            {
                await next(context).ConfigureAwait(false);
            }
            catch (UserResponseException exception)
            {
                await _userResponseExceptionHandler.HandleAsync(context, exception).ConfigureAwait(false);
            }
            catch (ProblemDetailsException exception)
            {
                // Berechtigung auf Global Admin checken
                await _problemDetailsExceptionHandler.HandleAsync(context, exception).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await _defaultExceptionHandler.HandleAsync(context, exception).ConfigureAwait(false);
            }
        }
    }
}
