using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal class RegistrationCheck
    {
        private readonly IServiceCollection _serviceCollection;

        public RegistrationCheck(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        internal bool IsAlreadyRegistered(Type type)
        {
            return _serviceCollection.Any(registration => registration.ServiceType == type && registration.ImplementationType == type);
        }
    }
}