using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public static class LogStrategyExtension
    {
        public static void AddLogStrategies(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddErrorLogStrategy();
            serviceCollection.AddSecurityExceptionExceptionLogStrategy();
            serviceCollection.AddProblemDetailsExceptionExceptionLogStrategy();
            serviceCollection.AddDefaultExceptionExceptionLogStrategy();
        }
    }
}
