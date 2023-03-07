using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    public static class AddErrorHandlingStrategyExtensions
    {
        public static void AddErrorHandlingStrategy(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IErrorHandlingStrategy, ErrorHandlingStrategy>();
        }
    }

    public interface IErrorHandlingStrategy
    {
        Task HandleAsync(HttpContext context, Exception exception);
    }

    internal sealed class ErrorHandlingStrategy : IErrorHandlingStrategy
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
