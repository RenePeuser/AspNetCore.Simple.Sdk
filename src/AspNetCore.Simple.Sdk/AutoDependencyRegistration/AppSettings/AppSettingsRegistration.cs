using System;
using AspNetCore.Simple.Sdk.ErrorHandling;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal class AppSettingsRegistration : IRegistrationStrategy
    {
        private readonly LifetimeDetector _lifetimeDetector;
        private readonly IServiceCollection _serviceCollection;
        private readonly IConfiguration _configuration;

        public AppSettingsRegistration(LifetimeDetector lifetimeDetector, IServiceCollection serviceCollection, IConfiguration configuration)
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
            if (appsettingsAttribute.IsNull())
            {
                return false;
            }

            var settings = _configuration.GetSection(appsettingsAttribute.AppSettingsName).Get(appsettingsAttribute.SettingsType);
            var instance = Activator.CreateInstance(appsettingsAttribute.Validator);
            if (instance.IsNull())
            {
                throw new ProblemDetailsException("Was not able to create an instance of expected validator",
                                                  $"The type: '{appsettingsAttribute.Validator.Name}' could not be created");
            }

            var validator = instance.Cast<ISettingsValidatorBase>();
            validator.ValidateBase(settings);
            var lifetime = _lifetimeDetector.DetectFor(type);
            _serviceCollection.Add(new ServiceDescriptor(type, _ => settings, lifetime));
            return true;
        }
    }
}
