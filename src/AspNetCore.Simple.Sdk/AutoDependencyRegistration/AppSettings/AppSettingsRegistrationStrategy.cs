using System;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal class AppSettingsRegistrationStrategy : IRegistrationStrategy
    {
        private readonly LifetimeDetector _lifetimeDetector;
        private readonly IServiceCollection _serviceCollection;
        private readonly IConfiguration _configuration;

        public AppSettingsRegistrationStrategy(LifetimeDetector lifetimeDetector, IServiceCollection serviceCollection, IConfiguration configuration)
        {
            _lifetimeDetector = lifetimeDetector;
            _serviceCollection = serviceCollection;
            _configuration = configuration;
        }

        public bool DoAutoRegistration(Type type, bool registrationDone)
        {
            if (registrationDone)
            {
                return true;
            }

            var appsettingsAttribute = type.GetCustomAttribute<AppSettingsRegistrationAttribute>();
            if (appsettingsAttribute.IsNull())
            {
                return false;
            }

            var settings = _configuration.GetSection(appsettingsAttribute.AppSettingsName).Get(appsettingsAttribute.SettingsType);
            var validator = Activator.CreateInstance(appsettingsAttribute.Validator).Cast<ISettingsValidatorBase>();
            validator.ValidateBase(settings);
            var lifetime = _lifetimeDetector.DetectFor(type);
            _serviceCollection.Add(new ServiceDescriptor(type, _ => settings, lifetime));
            return true;
        }
    }
}
