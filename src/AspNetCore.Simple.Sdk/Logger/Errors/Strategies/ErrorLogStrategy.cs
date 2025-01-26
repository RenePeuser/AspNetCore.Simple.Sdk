using System;
using System.Collections.Generic;
using System.Linq;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public static class AddErrorLogStrategyExtension
    {
        public static void AddErrorLogStrategy(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IErrorLogStrategy, ErrorLogStrategy>();
        }
    }

    public interface IErrorLogStrategy
    {
        void Handle(HttpContext context, Exception exception);
    }

    public class ErrorLogStrategy(IEnumerable<ISpecificErrorLogStrategy> specificErrorLogStrategies,
                                  ILogger<ErrorLogStrategy> logger) : IErrorLogStrategy
    {
        public void Handle(HttpContext context, Exception exception)
        {
            var result = specificErrorLogStrategies.Aggregate(false, (current, specificErrorLogStrategy) => specificErrorLogStrategy.HandleException(context, exception, current));
            if (result.IsFalse())
            {
                logger.LogError($"No strategy handled exception: {exception.GetType()}. Following exception occurred: {exception.Message}");
            }
        }
    }
}
