using System;
using System.Linq;
using System.Reflection;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal sealed class SimpleAppSettingsRegistration : IRegistrationStrategy
    {
        private readonly LifetimeDetector _lifetimeDetector;
        private readonly IServiceCollection _serviceCollection;
        private readonly IConfiguration _configuration;
        private readonly BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        public SimpleAppSettingsRegistration(LifetimeDetector lifetimeDetector, IServiceCollection serviceCollection, IConfiguration configuration)
        {
            _lifetimeDetector = lifetimeDetector;
            _serviceCollection = serviceCollection;
            _configuration = configuration;
        }

        public bool DoRegistrationFor(Type type, bool registrationDone)
        {
            if (registrationDone)
            {
                return true;
            }

            var appsettingsAttribute = type.GetCustomAttribute<AppSettingsRegistrationAttribute>();
            if (appsettingsAttribute.IsNotNull())
            {
                return false;
            }

            var serviceAttribute = type.GetCustomAttribute<ServiceRegistrationAttribute>();
            if (serviceAttribute.IsNotNull())
            {
                return false;
            }

            // Specific logic, if we have a pure data structure which will be injected we will check
            // 1. If is a pure data class, if we find explicit declared methods this can not be a settings class (simple one)
            var methods = type.GetMethods(_bindingFlags).Where(m => m.IsSpecialName.IsFalse());
            if (methods.Any())
            {
                return false;
            }

            // if a settings class does not have properties, and do not have explicit registration information we can not register it as settings
            var properties = type.GetProperties(_bindingFlags);
            if (properties.IsEmpty())
            {
                return false;
            }

            // 3. Check class name first
            var settingsName = type.Name;
            var settings = _configuration.GetSection(settingsName).Get(type);

            //// 4. Fallback value check if name contains settings remove it
            //if (settings is null)
            //{
            //    settingsName = SettingsPostFixToReplace.Aggregate(settingsName, (current, stringToReplace) => current.Replace(stringToReplace, string.Empty));
            //    settings = _configuration.GetSection(settingsName).Get(type);
            //}

            // 5. If settings is still null, then we don't know it
            if (settings is null)
            {
                return false;
            }

            var lifetime = _lifetimeDetector.DetectFor(type);
            _serviceCollection.Add(new ServiceDescriptor(type, _ => settings, lifetime));
            return true;
        }
    }
}
