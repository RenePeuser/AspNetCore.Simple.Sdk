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

    internal sealed class ErrorHandlingStrategy(IEnumerable<ISpecificErrorHandler> specificErrorHandlers) : IErrorHandlingStrategy
    {
        public async Task HandleAsync(HttpContext context, Exception exception)
        {
            var lastResult = false;
            foreach (var specificErrorLogStrategy in specificErrorHandlers)
            {
                lastResult = await specificErrorLogStrategy.HandleExceptionAsync(context, exception, lastResult).ConfigureAwait(false);
            }
        }
    }
}
