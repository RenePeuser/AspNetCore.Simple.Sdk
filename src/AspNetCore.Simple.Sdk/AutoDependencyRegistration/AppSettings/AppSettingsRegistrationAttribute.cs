using System;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AppSettingsRegistrationAttribute : ServiceRegistrationAttribute
    {
        public AppSettingsRegistrationAttribute(string appSettingsName,
                                                Type settingsType,
                                                ServiceLifetime serviceLifetime) : this(appSettingsName, settingsType, serviceLifetime, typeof(DefaultValidator))
        {

        }

        public AppSettingsRegistrationAttribute(string appSettingsName, Type settingsType, ServiceLifetime serviceLifetime, Type validator) : base(serviceLifetime)
        {
            AppSettingsName = appSettingsName;
            SettingsType = settingsType;
            Validator = validator;

            if (typeof(ISettingsValidatorBase).IsAssignableFrom(validator).IsFalse())
            {
                throw new InvalidValidatorTypeException(validator);
            }
        }

        public string AppSettingsName { get; }

        public Type SettingsType { get; }

        public Type Validator { get; }
    }
}