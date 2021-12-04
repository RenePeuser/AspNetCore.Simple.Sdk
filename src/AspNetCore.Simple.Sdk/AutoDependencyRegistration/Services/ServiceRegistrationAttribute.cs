using System;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceRegistrationAttribute : Attribute
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