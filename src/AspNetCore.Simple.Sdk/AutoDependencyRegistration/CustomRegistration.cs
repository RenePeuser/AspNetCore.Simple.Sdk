using System;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal class CustomRegistration : IRegistrationStrategy
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

            var customRegistration = Activator.CreateInstance(customRegistrationAttribute.CustomRegistration).Cast<ICustomTypeRegistration>();
            customRegistration.Register(_serviceCollection, _configuration);
            return true;
        }
    }
}
