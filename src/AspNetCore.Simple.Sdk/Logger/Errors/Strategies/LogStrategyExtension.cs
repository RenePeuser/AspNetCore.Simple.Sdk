using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public static class LogStrategyExtension
    {
        public static void AddLogStrategies(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingletonIfNotExists<IErrorLogStrategy, ErrorLogStrategy>();
            serviceCollection.AddSingletonIfNotExists<ISpecificErrorLogStrategy, SecurityExceptionExceptionLogStrategy>();
            serviceCollection.AddSingletonIfNotExists<ISpecificErrorLogStrategy, ProblemDetailsExceptionExceptionLogStrategy>();
            serviceCollection.AddSingletonIfNotExists<ISpecificErrorLogStrategy, DefaultExceptionExceptionLogStrategy>();
        }
    }
}
