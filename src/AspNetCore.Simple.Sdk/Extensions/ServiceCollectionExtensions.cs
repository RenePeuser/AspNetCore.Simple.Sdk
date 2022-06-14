using System;
using System.Collections.Generic;
using System.Linq;
using AspNetCore.Simple.Sdk.ErrorHandling;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.Extensions
{
    public class MissingSettingsException<TSettings> : Exception where TSettings : class
    {
        public MissingSettingsException() : base($"The setting: '{typeof(TSettings).Name}' is missing. Please check your specific appsettings.json or your environment variables.")
        {

        }
    }

    public static class ServiceCollectionExtensions
    {
        public static bool IsAlreadyRegistered<TImplementation>(this IServiceCollection services)
            where TImplementation : class
        {
            var existingRegistrations = services.Where(descriptor => descriptor.ServiceType == typeof(TImplementation) || descriptor.ImplementationType == typeof(TImplementation));
            return existingRegistrations.Any();
        }


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

        public static void AddSingletonIfNotExists<TImplementation>(this IServiceCollection services)
            where TImplementation : class
        {
            services.AddSingletonIfNotExists<TImplementation, TImplementation>();
        }

        public static void AddSingletonIfNotExists<TImplementation>(this IServiceCollection services, TImplementation instance)
            where TImplementation : class
        {
            var existingRegistrations = services.Where(descriptor => descriptor.ServiceType == typeof(TImplementation) && descriptor.ImplementationType == typeof(TImplementation));
            if (existingRegistrations.Any())
            {
                return;
            }

            services.AddSingleton(instance);
        }

        public static void AddSingletonIfNotExists<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            var existingRegistrations = services.Where(descriptor => descriptor.ServiceType == typeof(TInterface) && descriptor.ImplementationType == typeof(TImplementation));
            if (existingRegistrations.Any())
            {
                return;
            }

            services.AddSingleton(typeof(TInterface), typeof(TImplementation));
        }

        public static T GetSettings<T>(this IConfiguration configuration) where T : class, new()
        {
            var originalTypeSettings = configuration.TryGetSettings<T>(out var settings);
            if (originalTypeSettings)
            {
                return settings;
            }

            var typeNameTrimmedSettings = typeof(T).NormalizeTypeNameForSettings();
            return configuration.GetSettings<T>(typeNameTrimmedSettings);

        }

        public static T GetSettings<T>(this IConfiguration configuration, string settingsKeyPath)
            where T : class
        {
            var settings = configuration.GetSection(settingsKeyPath).Get<T>();
            if (settings is null)
            {
                throw new MissingSettingsException<T>();
            }

            return settings;
        }

        public static bool TryGetSettings<T>(this IConfiguration configuration, out T settings) where T : new()
        {
            // original settings by type name
            var type = typeof(T);
            var settingsByTypeExists = configuration.TryGetSettings(type.Name, out settings);
            return settingsByTypeExists is false ? configuration.TryGetSettings(type.NormalizeTypeNameForSettings(), out settings) : settingsByTypeExists;
        }

        private static string NormalizeTypeNameForSettings(this Type type)
        {
            return type.Name.Replace("Settings", string.Empty);
        }

        public static bool TryGetSettings<T>(this IConfiguration configuration, string settingsKeyPath, out T settings) where T : new()
        {
            var section = configuration.GetSection(settingsKeyPath).Get<T>();
            if (section is null)
            {
                settings = new T();
                return false;
            }

            settings = section;
            return true;
        }

        public static T GetSettingsAndRegisterAsSingleton<T>(this IServiceCollection serviceCollection,
                                                             IConfiguration configuration,
                                                             string? settingsKeyPath = null) where T : class, new()
        {
            var setting = settingsKeyPath is null ? configuration.GetSettings<T>() : configuration.GetSettings<T>(settingsKeyPath);

            serviceCollection.AddSingleton(setting);

            return setting;
        }
    }
}
