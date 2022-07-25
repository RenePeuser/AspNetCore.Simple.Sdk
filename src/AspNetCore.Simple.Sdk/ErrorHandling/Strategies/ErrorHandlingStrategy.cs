using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    internal class ErrorHandlingStrategy : IErrorHandlingStrategy
    {
        private readonly IEnumerable<ISpecificErrorHandler> _specificErrorHandlers;

        public ErrorHandlingStrategy(IEnumerable<ISpecificErrorHandler> specificErrorHandlers)
        {
            _specificErrorHandlers = specificErrorHandlers;
        }

        public async Task HandleAsync(HttpContext context, Exception exception)
        {
            var lastResult = false;
            foreach (var specificErrorLogStrategy in _specificErrorHandlers)
            {
                lastResult = await specificErrorLogStrategy.HandleExceptionAsync(context, exception, lastResult).ConfigureAwait(false);
            }
        }
    }
}
