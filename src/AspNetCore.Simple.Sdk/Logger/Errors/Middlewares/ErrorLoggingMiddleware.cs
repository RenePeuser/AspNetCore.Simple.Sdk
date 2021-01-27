using System;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.Logger.Errors.Strategies;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.Logger.Errors.Middlewares
{
    internal class ErrorLoggingMiddleware : IMiddleware
    {
        private readonly IErrorLogStrategy _errorLogStrategy;

        public ErrorLoggingMiddleware(IErrorLogStrategy errorLogStrategy)
        {
            _errorLogStrategy = errorLogStrategy;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _errorLogStrategy.Handle(context, exception);
                throw;
            }
        }
    }
}
