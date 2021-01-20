using AspNetCore.Simple.Sdk.Logger.Errors.Strategies.Specific;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Logger.Errors.Strategies
{
    public static class LogStrategyExtension
    {
        public static void AddLogStrategies(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IErrorLogStrategy, ErrorLogStrategy>();

            // very important is here the order !!!! DefaultExceptionLogStrategy should be the last one
            // it is like a switch case here
            serviceCollection.AddSingleton<ISpecificErrorLogStrategy, SecurityExceptionLogStrategy>();
            serviceCollection.AddSingleton<ISpecificErrorLogStrategy, ProblemDetailsExceptionLogStrategy>();

            serviceCollection.AddSingleton<ISpecificErrorLogStrategy, DefaultExceptionLogStrategy>();
        }
    }
}