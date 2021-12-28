using System;
using System.Linq;
using System.Reflection;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal class SimpleAppSettingsRegistration : IRegistrationStrategy
    {
        private readonly LifetimeDetector _lifetimeDetector;
        private readonly IServiceCollection _serviceCollection;
        private readonly IConfiguration _configuration;

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
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (methods.Any())
            {
                return false;
            }

            // 3. Check class name first
            var settingsName = type.Name;
            var settings = _configuration.GetSection(settingsName).Get(appsettingsAttribute.SettingsType);
            var lifetime = _lifetimeDetector.DetectFor(type);
            _serviceCollection.Add(new ServiceDescriptor(type, _ => settings, lifetime));
            return true;
        }
    }
}
