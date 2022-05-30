using System;
using System.Linq;
using AspNetCore.Simple.Sdk.ErrorHandling;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static T GetOrThrowMissingException<T>(this IServiceProvider services)
        {
            var service = services.GetService<T>();
            if (service is null)
            {
                throw new ProblemDetailsException("Service could not be resolved",
                                                  $"The service: {typeof(T).Name} could not be resolved please check your service registrations",
                                                  ("One time registration", $"services.{nameof(AddSingletonIfNotExists)}<{typeof(T).Name}>();"),
                                                  ("Standard registration", $"services.AddSingleton<{typeof(T).Name}>();"));
            }

            return service;
        }

        public static void AddSingletonOption<T>(this IServiceCollection serviceCollection, IConfiguration configuration) where T : class
        {
            var setting = configuration.GetSetting<T>();
            if (setting.IsNull())
            {
                throw new ProblemDetailsException(StatusCodes.Status500InternalServerError,
                                                  $"Setting of type: {typeof(T).Name} could not be found",
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
            var existingRegistrations = services.Where(descriptor => descriptor.ServiceType == typeof(TInterface) && descriptor.ImplementationType == typeof(TImplementation));
            if (existingRegistrations.Any())
            {
                return services;
            }

            services.AddSingleton(typeof(TInterface), typeof(TImplementation));
            return services;
        }
    }
}
