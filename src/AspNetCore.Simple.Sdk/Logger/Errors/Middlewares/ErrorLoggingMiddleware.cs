using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    internal sealed class ErrorLoggingMiddleware(IErrorLogStrategy errorLogStrategy) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                errorLogStrategy.Handle(context, exception);
                throw;
            }
        }
    }
}
