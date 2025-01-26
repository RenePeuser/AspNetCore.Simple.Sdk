using System;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    public class ServiceRegistrationAttribute(ServiceLifetime serviceLifetime,
                                              Type? interfaceType = default) : RegistrationBaseAttribute
    {
        public ServiceLifetime ServiceLifetime { get; } = serviceLifetime;

        public Type? InterfaceType { get; } = interfaceType;
    }
}
