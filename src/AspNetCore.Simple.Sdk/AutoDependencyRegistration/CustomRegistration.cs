using System;
using AspNetCore.Simple.Sdk.ErrorHandling;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal sealed class CustomRegistration : IRegistrationStrategy
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IConfiguration _configuration;

        public CustomRegistration(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            _serviceCollection = serviceCollection;
            _configuration = configuration;
        }

        public bool DoRegistrationFor(Type type, bool registrationDone)
        {
            if (registrationDone)
            {
                return true;
            }

            var customRegistrationAttribute = type.GetCustomAttribute<CustomRegistrationAttribute>();
            if (customRegistrationAttribute is null)
            {
                return false;
            }

            var instance = Activator.CreateInstance(customRegistrationAttribute.CustomRegistration);
            if (instance.IsNull())
            {
                throw new ProblemDetailsException("Was not able to create an instance of expected type for auto registration",
                                                  $"The type: '{customRegistrationAttribute.CustomRegistration.Name}' could not be created");
            }

            var customRegistration = instance.Cast<ICustomTypeRegistration>();
            customRegistration.Register(_serviceCollection, _configuration);
            return true;
        }
    }
}
