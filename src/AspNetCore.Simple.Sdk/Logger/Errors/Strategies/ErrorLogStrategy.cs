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

    public class ErrorLogStrategy : IErrorLogStrategy
    {
        private readonly IEnumerable<ISpecificErrorLogStrategy> _specificErrorLogStrategies;
        private readonly ILogger<ErrorLogStrategy> _logger;

        public ErrorLogStrategy(IEnumerable<ISpecificErrorLogStrategy> specificErrorLogStrategies, ILogger<ErrorLogStrategy> logger)
        {
            _specificErrorLogStrategies = specificErrorLogStrategies;
            _logger = logger;
        }

        public void Handle(HttpContext context, Exception exception)
        {
            var result = _specificErrorLogStrategies.Aggregate(false, (current, specificErrorLogStrategy) => specificErrorLogStrategy.HandleException(context, exception, current));
            if (result.IsFalse())
            {
                _logger.LogError($"No strategy handled exception: {exception.GetType()}. Following exception occurred: {exception.Message}");
            }
        }
    }
}
