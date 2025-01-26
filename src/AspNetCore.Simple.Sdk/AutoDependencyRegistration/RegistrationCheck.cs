using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    internal sealed class RegistrationCheck(IServiceCollection serviceCollection)
    {
        internal bool IsAlreadyRegistered(Type type)
        {
            var isAlreaydRegistered = serviceCollection.Any(registration => registration.ServiceType == type || registration.ImplementationType == type);
            return isAlreaydRegistered;
        }
    }
}
