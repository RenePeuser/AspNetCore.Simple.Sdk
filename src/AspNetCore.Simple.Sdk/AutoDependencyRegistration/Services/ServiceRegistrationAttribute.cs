using System;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    public class ServiceRegistrationAttribute : RegistrationBaseAttribute
    {
        public ServiceRegistrationAttribute(ServiceLifetime serviceLifetime, Type? interfaceType = default)
        {
            ServiceLifetime = serviceLifetime;
            InterfaceType = interfaceType;
        }

        public ServiceLifetime ServiceLifetime { get; }

        public Type? InterfaceType { get; }
    }
}
