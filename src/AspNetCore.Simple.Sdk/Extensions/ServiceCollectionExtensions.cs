using AspNetCore.Simple.Sdk.ErrorHandling;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSingletonOption<T>(this IServiceCollection serviceCollection, IConfiguration configuration) where T : class
        {
            var setting = configuration.GetSetting<T>();
            if (setting.IsNull())
            {
                throw new ProblemDetailsException(StatusCodes.Status500InternalServerError, $"Setting of type: {typeof(T).Name} could not be found",
                    $"Please check your appsettings.json, or check if the name of your class '{typeof(T).Name}' mach the section name in your appsettings.json");
            }

            serviceCollection.AddSingleton(setting);
        }
    }
}
