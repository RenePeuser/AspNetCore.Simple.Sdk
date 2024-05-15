using System;
using AspNetCore.Simple.Sdk.ErrorHandling;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal sealed class AppSettingsRegistration : IRegistrationStrategy
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
            if (settings.IsNull())
            {
                throw new ProblemDetailsException("Was not able to get settings type info",
                    $"The settings: '{appsettingsAttribute.AppSettingsName}__{appsettingsAttribute.SettingsType}' was not found");
            }

            var instance = Activator.CreateInstance(appsettingsAttribute.Validator);
            if (instance.IsNull())
            {
                throw new ProblemDetailsException("Was not able to create an instance of expected validator",
                    $"The type: '{appsettingsAttribute.Validator.Name}' could not be created");
            }

            var validator = instance.As<ISettingsValidatorBase>();
            if (validator.IsNull())
            {
                throw new ProblemDetailsException("Was not able to create an instance of expected validator",
                    $"The type: '{appsettingsAttribute.Validator.Name}' could not be created");
            }

            validator.ValidateBase(settings);
            var lifetime = _lifetimeDetector.DetectFor(type);
            _serviceCollection.Add(new ServiceDescriptor(type, _ => settings, lifetime));
            return true;
        }
    }
}
