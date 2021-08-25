using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public static IServiceCollection AddSingletonIfNotExists<TImplementation>(this IServiceCollection services)
            where TImplementation : class
        {
            return services.AddSingletonIfNotExists<TImplementation, TImplementation>();
        }

        public static IServiceCollection AddSingletonIfNotExists<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            var fieldInfo = services.GetType().GetField("_descriptors", BindingFlags.NonPublic | BindingFlags.Instance);
            var _descriptors = fieldInfo.GetValue(services).Cast<List<ServiceDescriptor>>();
            var existingRegistrations = _descriptors.Where(descriptor => descriptor.ServiceType == typeof(TInterface) && descriptor.ImplementationType == typeof(TImplementation));
            if (existingRegistrations.Any())
            {
                return services;
            }

            services.AddSingleton(typeof(TInterface), typeof(TImplementation));
            return services;
        }
    }
}
