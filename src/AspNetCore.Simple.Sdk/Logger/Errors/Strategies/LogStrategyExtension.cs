using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public static class LogStrategyExtension
    {
        public static void AddLogStrategies(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IErrorLogStrategy, ErrorLogStrategy>();
            serviceCollection.AddSingleton<ISpecificErrorLogStrategy, SecurityExceptionExceptionLogStrategy>();
            serviceCollection.AddSingleton<ISpecificErrorLogStrategy, ProblemDetailsExceptionExceptionLogStrategy>();
            serviceCollection.AddSingleton<ISpecificErrorLogStrategy, DefaultExceptionExceptionLogStrategy>();
        }
    }
}
