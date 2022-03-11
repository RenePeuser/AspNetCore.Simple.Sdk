using System;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    public interface ICustomTypeRegistration
    {
        void Register(IServiceCollection serviceCollection, IConfiguration configuration);
    }


    public class CustomRegistrationAttribute : Attribute
    {
        public CustomRegistrationAttribute(Type customRegistration)
        {
            if (customRegistration is null)
            {
                throw new ArgumentException("Your custom registration must not be null.");
            }

            if (customRegistration.IsInterface)
            {
                throw new ArgumentException("Your custom registration type must not be an interface.");
            }

            if (typeof(ICustomTypeRegistration).IsAssignableFrom(customRegistration).IsFalse())
            {
                throw new ArgumentException($"Your custom registration type have to be derive from: '{nameof(ICustomTypeRegistration)}'");
            }

            CustomRegistration = customRegistration;
        }

        public Type CustomRegistration { get; }
    }


    [AttributeUsage(AttributeTargets.Class)]
    public class AppSettingsRegistrationAttribute : ServiceRegistrationAttribute
    {
        public AppSettingsRegistrationAttribute(string appSettingsName,
                                                Type settingsType,
                                                ServiceLifetime serviceLifetime) : this(appSettingsName, settingsType, serviceLifetime, typeof(DefaultValidator))
        {

        }

        public AppSettingsRegistrationAttribute(string appSettingsName,
                                                Type settingsType,
                                                ServiceLifetime serviceLifetime,
                                                Type validator) : base(serviceLifetime)
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
